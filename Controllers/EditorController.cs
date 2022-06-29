using Microsoft.AspNetCore.Mvc;
using nng.Enums;
using nng.VkFrameworks;
using nng_bot.API;
using nng_bot.Frameworks;
using nng_bot.Models;
using VkNet.Model;
using VkNet.Utils;
using static nng_bot.API.KeyBoardFramework;
using static nng_bot.Configs.UsersConfigurationProcessor;

namespace nng_bot.Controllers;

[ApiController]
[Route("")]
public partial class EditorController : ControllerBase
{
    public EditorController(VkController vkController, OperationStatus status, ILogger<EditorController> logger,
        IConfiguration configuration, PhraseFramework phraseFramework, CacheFramework cacheFramework,
        CacheScheduledTaskProcessor cacheScheduledTaskProcessor, VkFramework vkFramework)
    {
        Logger = logger;
        Configuration = configuration;
        PhraseFramework = phraseFramework;
        CacheFramework = cacheFramework;
        CacheScheduledTaskProcessor = cacheScheduledTaskProcessor;
        VkFramework = vkFramework;
        VkController = vkController;
        Status = status;
    }

    private IConfiguration Configuration { get; }
    private ILogger<EditorController> Logger { get; }
    private VkController VkController { get; }
    private PhraseFramework PhraseFramework { get; }
    private CacheFramework CacheFramework { get; }
    private OperationStatus Status { get; }
    private VkFramework VkFramework { get; }
    private CacheScheduledTaskProcessor CacheScheduledTaskProcessor { get; }

    private bool IsAllowed(long group, string secret)
    {
        return Configuration.GetValue<long>("Auth:DialogGroupId").Equals(group) &&
               Configuration["Auth:DialogGroupSecret"].Equals(secret);
    }

    private static string GetBannedKeyboard(BanPriority priority)
    {
        return priority is not BanPriority.Red and not BanPriority.Green and not BanPriority.White
            ? RestrictedStartButtons
            : RestrictedStartButtonsWithUnbanRequest;
    }

    private string GetStartMenuKeyboard(long id)
    {
        if (CacheFramework.TryGetBannedPriority(id, out var priority)) return GetBannedKeyboard(priority);
        return IfUserIsAdmin(id) ? AdminStartButtons : StartButtons;
    }

    private async Task DialogProcessor(long? peer, long? userId, Message message)
    {
        if (peer == null || userId == null)
        {
            Logger.LogWarning("Пустой вызов метода DialogProcessor");
            return;
        }

        var dialog = (long) peer;
        var user = (long) userId;
        if (Status.BackingTaskInProgress)
        {
            Status.UsersBotIsAvailable.Add(user);
            VkController.SendMessage(PhraseFramework.TempUnavailable,
                GoToMenuButtons, dialog);
            return;
        }

        try
        {
            ProcessAdminRequestMain(dialog, user, message);
            return;
        }
        catch (InvalidOperationException)
        {
            // ignored
        }

        if (message.Payload == null)
        {
            VkController.SendMessage(PhraseFramework.CommandNotFound,
                GoToMenuButtons, dialog);
            return;
        }

        if (PayloadAdminActions.IsAdminPayload(message.Payload) && !IfUserIsAdmin(user))
        {
            VkController.SendMessage(PhraseFramework.Error("NRE"),
                GoToMenuButtons, dialog);
            return;
        }

        switch (message.Payload)
        {
            case PayloadTemplates.ReturnBack:
            case PayloadTemplates.StartDialog:
                if (Status.UsersToEditorGiving.Any(x => x.User == user))
                    Status.UsersToEditorGiving.RemoveWhere(x => x.User == user);
                VkController.SendMessage(PhraseFramework.MainMenu,
                    GetStartMenuKeyboard(user), dialog);
                break;

            case PayloadTemplates.MyProfile:
                var priority = IfUserPrioritized(user);
                var profile = CacheFramework.LoadProfile(user);
                var userMessage = PhraseFramework.FormProfile(profile, priority);
                VkController.SendMessage(userMessage, GoToMenuButtons, dialog);
                break;

            case PayloadTemplates.GiveEditorPayload:
                if (!Configuration.GetValue<bool>("EditorGrantEnabled"))
                {
                    VkController.SendMessage(PhraseFramework.NoAvailableSlots,
                        GoToMenuButtons, dialog);
                    return;
                }

                if (CacheFramework.LoadProfile(user).EditorGroups.Length > 0)
                {
                    ProcessGiveEditor(user);
                    break;
                }

                VkController.SendMessage(PhraseFramework.AgreeWithRules,
                    AgreeWithRulesButtons, dialog);
                break;

            case PayloadAdminActions.LimitPanel:
                VkController.SendMessage(PhraseFramework.LimitPanel,
                    AdminLimitsButtons, user);
                break;

            case PayloadTemplates.UnbanMe:
                ProcessUnBanRequest(user);
                break;

            case PayloadTemplates.UnbanMeRequest:
                ProcessUnBanMe(user);
                break;

            case PayloadTemplates.ForceGiveEditorPayload:
                ProcessGiveEditor(user);
                break;

            case PayloadTemplates.IveJoined:
                await ProcessIveJoined(user);
                break;

            case PayloadTemplates.IveJoinedInFiftySubs:
                ProcessIveJoinedLessThanFiftySubs(user);
                break;

            case PayloadAdminActions.AdminPanel:
                ProcessAdminPanelEnter(user);
                break;

            case PayloadAdminActions.BanUser:
                ProcessAdminBanUser(user);
                break;

            case PayloadAdminActions.MakeUserAdmin:
                ProcessAdminMakeAdmin(user);
                break;

            case PayloadAdminActions.MakeUserLimitless:
                ProcessAdminMakeLimitless(user);
                break;

            case PayloadAdminActions.GroupsStatistics:
                ProcessAdminGetStatistics(user);
                break;

            case PayloadAdminActions.ClearCache:
                ProcessAdminClearCache(user);
                break;

            case PayloadAdminActions.EditEditorRestriction:
                ProcessAdminEditEditorRestrictions(user);
                break;

            case PayloadAdminActions.ShowOtherUserProfile:
                ProcessAdminShowOtherUserProfile(user);
                break;

            #region UnbanRequests

            case PayloadAdminActions.UnbanRequests:
                ProcessAdminPanelUnbanRequests(user);
                break;

            case PayloadAdminActions.UnbanRequestMoveForward:
                ProcessAdminPanelUnbanRequestMoveForward(user);
                break;

            case PayloadAdminActions.UnbanRequestMoveBack:
                ProcessAdminPanelUnbanRequestMoveBackward(user);
                break;

            case PayloadAdminActions.UnbanRequestAccept:
                ProcessAdminUnbanRequestAccept(user);
                break;

            case PayloadAdminActions.UnbanRequestDeny:
                ProcessAdminUnbanRequestDeny(user);
                break;

            case PayloadAdminActions.UnbanRequestDelete:
                ProcessUnbanRequestDelete(user);
                break;

            #endregion

            default:
                VkController.SendMessage(PhraseFramework.MainMenu,
                    StartButtons, dialog);
                break;
        }
    }

    [HttpPost]
    public async Task<IActionResult> Dialog([FromBody] VkEvent vkEvent)
    {
        if (!IsAllowed(vkEvent.GroupId, vkEvent.Secret)) return Ok("ok");

        if (vkEvent.Type == "confirmation") return Ok(Configuration["Auth:DialogGroupConfirm"]);

        if (vkEvent.Type == "message_new")
        {
            var message = Message.FromJson(new VkResponse(vkEvent.Object));
            Logger.LogInformation("message_new\n\tUser: {User}\n\tContent: «{Content}»",
                message.FromId, message.Text);
            await DialogProcessor(message.PeerId, message.FromId, message);
        }

        return Ok("ok");
    }
}
