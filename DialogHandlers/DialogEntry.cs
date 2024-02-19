using nng_bot;
using nng_bot.API;
using nng_bot.Frameworks;
using nng_bot.Providers;
using VkNet.Model;
using static nng_bot.Frameworks.KeyBoardFramework;
using User = nng.DatabaseModels.User;

namespace nng;

public class DialogEntry
{
    private readonly VkDialogPayloadHandler _payloadHandler;
    private readonly PhraseFramework _phraseFramework;

    private readonly OperationStatus _status;
    private readonly UserFramework _userFramework;
    private readonly VkController _vkController;

    public DialogEntry(OperationStatus status, VkController vkController, VkDialogPayloadHandler payloadHandler,
        UserFramework userFramework, PhraseFramework phraseFramework)
    {
        _status = status;
        _vkController = vkController;
        _payloadHandler = payloadHandler;
        _userFramework = userFramework;
        _phraseFramework = phraseFramework;
    }

    public void Enter(Message message)
    {
        if (message.FromId is null) throw new NullReferenceException("FromId is null");
        var user = (long) message.FromId;

        User? profile;

        try
        {
            profile = _userFramework.GetById(user);
        }
        catch (Exception)
        {
            profile = null;
        }

        if (message.Payload is null)
        {
            _vkController.SendMessage(PhraseFramework.CommandNotFound,
                GoToMenuButtons, user);
            return;
        }

        var regDate = UsersRegistrationDateProvider.GetRegistrationDate(user);

        switch (message.Payload)
        {
            case PayloadTemplates.StartDialog:
                _status.ClearRequests(user);
                _vkController.SendSticker(user, 60790);
                _vkController.SendMessage(PhraseFramework.MainMenu, VkDialogHelper.GetStartMenuKeyboard(profile), user);
                break;

            case PayloadTemplates.ReturnBack:
                _status.ClearRequests(user);
                _vkController.SendMessage(PhraseFramework.MainMenu, VkDialogHelper.GetStartMenuKeyboard(profile), user);
                break;

            case PayloadTemplates.MyProfile:
                var vkUser = _vkController.VkFramework.GetUser(user);
                var name = $"{vkUser.FirstName} {vkUser.LastName}";

                if (profile is null)
                {
                    _vkController.SendMessage(_phraseFramework.FormBlankProfile(user, name, regDate), GoToMenuButtons,
                        user);
                    return;
                }

                var userMessage = _phraseFramework.FormProfile(profile, regDate);
                _vkController.SendMessage(userMessage, GoToMenuButtons, user);
                break;

            case PayloadTemplates.GiveEditorPayload:
                if ((DateTime.Now - regDate)?.TotalDays < 180)
                {
                    _vkController.SendMessage(PhraseFramework.YourAccountIsTooYoung,
                        GoToMenuButtons, user);
                    return;
                }

                if (profile?.Groups is not null && profile.Groups.Any())
                {
                    _payloadHandler.GiveEditor(user, profile);
                    break;
                }

                _vkController.SendMessage(PhraseFramework.AgreeWithRules,
                    AgreeWithRulesButtons, user);
                break;

            case PayloadTemplates.ForceGiveEditorPayload:
                if ((DateTime.Now - regDate)?.TotalDays < 180)
                {
                    _vkController.SendMessage(PhraseFramework.YourAccountIsTooYoung,
                        GoToMenuButtons, user);
                    return;
                }

                _payloadHandler.GiveEditor(user, profile);
                break;

            case PayloadTemplates.IveJoined:
                _payloadHandler.Joined(user);
                break;

            case PayloadTemplates.IveJoinedInFiftySubs:
                _payloadHandler.JoinedLessThanFiftySubs(user);
                break;

            default:
                _vkController.SendMessage(PhraseFramework.MainMenu,
                    StartButtons, user);
                break;
        }
    }
}
