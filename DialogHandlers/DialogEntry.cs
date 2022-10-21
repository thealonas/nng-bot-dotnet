using nng_bot.API;
using nng_bot.Frameworks;
using VkNet.Model;
using static nng_bot.Configs.UsersConfigurationProcessor;
using static nng_bot.API.KeyBoardFramework;

namespace nng;

public class DialogEntry
{
    private readonly VkDialogAdminHandler _adminHandler;
    private readonly CacheFramework _cacheFramework;

    private readonly EnvironmentConfiguration _configuration;

    private readonly VkDialogHelper _dialogHelper;
    private readonly VkDialogPayloadHandler _payloadHandler;
    private readonly PhraseFramework _phraseFramework;

    private readonly OperationStatus _status;
    private readonly VkDialogUnbanRequests _unbanHandler;
    private readonly VkController _vkController;

    public DialogEntry(OperationStatus status, VkController vkController, VkDialogUnbanRequests unbanHandler,
        VkDialogAdminHandler adminHandler, VkDialogPayloadHandler payloadHandler, VkDialogHelper dialogHelper,
        PhraseFramework phraseFramework, CacheFramework cacheFramework)
    {
        _status = status;
        _vkController = vkController;
        _configuration = EnvironmentConfiguration.GetInstance();
        _unbanHandler = unbanHandler;
        _adminHandler = adminHandler;
        _payloadHandler = payloadHandler;
        _dialogHelper = dialogHelper;
        _phraseFramework = phraseFramework;
        _cacheFramework = cacheFramework;
    }

    public void Enter(Message message)
    {
        if (message.FromId is null) throw new NullReferenceException("FromId is null");
        var user = (long) message.FromId;

        if (_status.BackingTaskInProgress)
        {
            _status.UsersBotIsAvailable.Add(user);
            _vkController.SendMessage(_phraseFramework.TempUnavailable,
                GoToMenuButtons, user);
            return;
        }

        try
        {
            _adminHandler.AdminRequestMain(user, message);
            return;
        }
        catch (InvalidOperationException)
        {
            // ignored
        }

        if (message.Payload == null)
        {
            _vkController.SendMessage(_phraseFramework.CommandNotFound,
                GoToMenuButtons, user);
            return;
        }

        if (PayloadAdminActions.IsAdminPayload(message.Payload) && !IfUserIsAdmin(user))
        {
            _vkController.SendMessage(_phraseFramework.Error("NRE"),
                GoToMenuButtons, user);
            return;
        }

        switch (message.Payload)
        {
            case PayloadTemplates.StartDialog:
            case PayloadTemplates.ReturnBack:
                if (_status.UsersToEditorGiving.Any(x => x.User == user))
                    _status.UsersToEditorGiving.RemoveWhere(x => x.User == user);

                _vkController.SendMessage(_phraseFramework.MainMenu, _dialogHelper.GetStartMenuKeyboard(user), user);
                break;

            case PayloadTemplates.MyProfile:
                var priority = IfUserPrioritized(user);
                var profile = _cacheFramework.LoadProfile(user);
                var userMessage = _phraseFramework.FormProfile(profile, priority);
                _vkController.SendMessage(userMessage, GoToMenuButtons, user);
                break;

            case PayloadTemplates.GiveEditorPayload:
                if (!_configuration.Configuration.EditorGrantEnabled)
                {
                    _vkController.SendMessage(_phraseFramework.NoAvailableSlots,
                        GoToMenuButtons, user);
                    return;
                }

                if (_cacheFramework.LoadProfile(user).EditorGroups.Length > 0)
                {
                    _payloadHandler.GiveEditor(user);
                    break;
                }

                _vkController.SendMessage(_phraseFramework.AgreeWithRules,
                    AgreeWithRulesButtons, user);
                break;

            case PayloadTemplates.UnbanMe:
                _payloadHandler.SubmitUnBanRequest(user);
                break;

            case PayloadTemplates.UnbanMeRequest:
                _payloadHandler.UnBanMe(user);
                break;

            case PayloadTemplates.ForceGiveEditorPayload:
                _payloadHandler.GiveEditor(user);
                break;

            case PayloadTemplates.IveJoined:
                _payloadHandler.Joined(user);
                break;

            case PayloadTemplates.IveJoinedInFiftySubs:
                _payloadHandler.JoinedLessThanFiftySubs(user);
                break;

            case PayloadAdminActions.AdminPanel:
                _adminHandler.PanelEnter(user);
                break;

            case PayloadAdminActions.GroupsStatistics:
                _adminHandler.GetStatistics(user);
                break;

            case PayloadAdminActions.ClearCache:
                _adminHandler.ClearCache(user);
                break;

            case PayloadAdminActions.ClearBanned:
                _adminHandler.ClearBanned(user);
                break;

            case PayloadAdminActions.ShowOtherUserProfile:
                _adminHandler.ShowUserProfile(user);
                break;

            #region UnbanRequests

            case PayloadAdminActions.UnbanRequests:
                _unbanHandler.PanelUnbanRequests(user);
                break;

            case PayloadAdminActions.UnbanRequestMoveForward:
                _unbanHandler.UnbanRequestMoveForward(user);
                break;

            case PayloadAdminActions.UnbanRequestMoveBack:
                _unbanHandler.UnbanRequestMoveBackward(user);
                break;

            case PayloadAdminActions.UnbanRequestAccept:
                _unbanHandler.UnbanRequestAccept(user);
                break;

            case PayloadAdminActions.UnbanRequestDeny:
                _unbanHandler.UnbanRequestDeny(user);
                break;

            case PayloadAdminActions.UnbanRequestDelete:
                _unbanHandler.UnbanRequestDelete(user);
                break;

            #endregion

            default:
                _vkController.SendMessage(_phraseFramework.MainMenu,
                    StartButtons, user);
                break;
        }
    }
}
