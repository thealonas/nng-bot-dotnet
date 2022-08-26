using Newtonsoft.Json;
using nng_bot.API;
using nng_bot.Enums;
using nng_bot.Extensions;
using nng_bot.Frameworks;
using nng_bot.Models;
using nng.VkFrameworks;
using VkNet.Enums.SafetyEnums;
using VkNet.Exception;
using VkNet.Model.Keyboard;
using static nng_bot.API.KeyBoardFramework;

namespace nng;

public class VkDialogUnbanRequests
{
    private readonly CacheFramework _cacheFramework;
    private readonly ILogger<VkDialogUnbanRequests> _logger;
    private readonly PhraseFramework _phraseFramework;
    private readonly OperationStatus _status;
    private readonly VkController _vkController;
    private readonly VkFramework _vkFramework;

    public VkDialogUnbanRequests(OperationStatus status, VkController vkController,
        CacheFramework cacheFramework, VkFramework vkFramework, ILogger<VkDialogUnbanRequests> logger,
        PhraseFramework phraseFramework)
    {
        _status = status;
        _vkController = vkController;
        _cacheFramework = cacheFramework;
        _vkFramework = vkFramework;
        _logger = logger;
        _phraseFramework = phraseFramework;
    }

    public void UnbanRequestMoveForward(long user)
    {
        var index = _status.GetAdminCurrentUnbanRequestPageIndex(user) + 1;
        _status.SetAdminCurrentUnbanRequestPageIndex(user, index);

        var phrase = GetUnbanRequestOverview(user);
        var keyboard = GetUnbanRequestKeyboard(user);

        _vkController.SendMessage(phrase, keyboard, user);
    }

    public void UnbanRequestMoveBackward(long user)
    {
        var index = _status.GetAdminCurrentUnbanRequestPageIndex(user) - 1;
        _status.SetAdminCurrentUnbanRequestPageIndex(user, index);

        var phrase = GetUnbanRequestOverview(user);
        var keyboard = GetUnbanRequestKeyboard(user);

        _vkController.SendMessage(phrase, keyboard, user);
    }

    public void UnbanRequestAccept(long user)
    {
        var index = _status.GetAdminCurrentUnbanRequestPageIndex(user);
        var targetRequest = GetUnbanRequest(user, index);

        CacheFramework.SetBannedRequestStatus(targetRequest, UnbanRequestStatus.Accepted);
        CacheFramework.SetBannedWatchedStatus(targetRequest, true);

        _status.UnbannedUsers.Add(targetRequest.UserId);
        UnbanUserInAllGroups(user, targetRequest);
    }

    public void UnbanUserInAllGroups(long admin, UnbanRequest request)
    {
        var data = CacheFramework.LoadCache().Data;
        var id = request.UserId;
        foreach (var cacheGroup in data)
        {
            var banned = _vkFramework.GetBanned(cacheGroup.Id);
            if (banned.All(x => x.Id != request.UserId)) continue;
            var group = cacheGroup.Id;
            try
            {
                _vkFramework.UnBlock(group, id);
            }
            catch (VkApiException e)
            {
                _logger.LogError("Невозможно разбанить пользователя в группе {Group}: {Type}: {Message}",
                    cacheGroup.Id, e.GetType(), e.Message);
            }
        }

        _vkController.SendMessage(_phraseFramework.YouHaveBeenUnbanned,
            null, request.UserId);
        _vkController.SendMessage(_phraseFramework.UserHasBeenUnbanned(request.UserId),
            AdminPanelButtons, admin);
    }

    public void UnbanRequestDeny(long user)
    {
        var index = _status.GetAdminCurrentUnbanRequestPageIndex(user);
        var targetRequest = GetUnbanRequest(user, index);

        CacheFramework.SetBannedRequestStatus(targetRequest, UnbanRequestStatus.Rejected);
        CacheFramework.SetBannedWatchedStatus(targetRequest, true);

        _status.UnbannedUsers.RemoveIfExists(targetRequest.UserId);

        _vkController.SendMessage(_phraseFramework.YourRequestHasBeenRejected,
            null, targetRequest.UserId);
        _vkController.SendMessage(_phraseFramework.UserRequestHasBeenRejected(targetRequest.UserId),
            AdminPanelButtons, user);
    }

    public void UnbanRequestDelete(long user)
    {
        var requests = CacheFramework.BannedRequestsFull.ToList();
        var count = requests.RemoveAll(x => x.Status is UnbanRequestStatus.Accepted or UnbanRequestStatus.Rejected
                                            || x.IsWatched || !_cacheFramework.IsBanned(x.UserId));
        requests = requests.ToList();
        CacheFramework.SaveBannedRequests(requests);
        _vkController.SendMessage(_phraseFramework.DeletedUnbanRequests($"{count}/{requests.Count + count}"),
            GoToAdminUnbanRequests, user);
    }

    public string GetUnbanRequestOverview(long adminId)
    {
        var index = _status.GetAdminCurrentUnbanRequestPageIndex(adminId);
        var requests = CacheFramework.BannedRequestsFull.Where(x => !x.IsWatched).ToList();

        var targetRequest = GetUnbanRequest(adminId, index);

        var phrase = _phraseFramework.UnbanRequestAdminOverview($"{index + 1}/{requests.Count}",
            targetRequest.Status.GetName(), targetRequest.CreatedOn.ToString("dd.MM.yyyy HH:mm:ss"),
            _phraseFramework.FormProfile(_cacheFramework.LoadProfile(targetRequest.UserId), false));
        return phrase;
    }

    public UnbanRequest GetUnbanRequest(long user, int index)
    {
        var unbanRequests = CacheFramework.BannedRequestsFull.Where(x => !x.IsWatched).ToList();
        UnbanRequest targetRequest;
        try
        {
            targetRequest = unbanRequests[index];
        }
        catch (ArgumentOutOfRangeException)
        {
            _logger.LogError(
                "Попытка получить запрос на разбан пользователя с индексом {Index} в коллекции запросов на разбан пользователей произошла в методе {Method}",
                index, nameof(GetUnbanRequest));
            _status.SetAdminCurrentUnbanRequestPageIndex(user, 0);
            targetRequest = unbanRequests[0];
        }

        return targetRequest;
    }

    public string GetUnbanRequestKeyboard(long adminId)
    {
        var index = _status.GetAdminCurrentUnbanRequestPageIndex(adminId);
        var requests = CacheFramework.BannedRequestsFull.Where(x => !x.IsWatched).ToList();
        var keyboard = new KeyboardBuilder();
        keyboard.SetOneTime();
        keyboard.AddButton(new AddButtonParams
        {
            ActionType = KeyboardButtonActionType.Text,
            Color = KeyboardButtonColor.Default,
            Extra = "UnbanRequestAccept",
            Type = "command",
            Label = "Принять"
        });
        keyboard.AddButton(new AddButtonParams
        {
            ActionType = KeyboardButtonActionType.Text,
            Color = KeyboardButtonColor.Default,
            Extra = "UnbanRequestDeny",
            Type = "command",
            Label = "Отклонить"
        });
        keyboard.AddButton(new AddButtonParams
        {
            ActionType = KeyboardButtonActionType.Text,
            Color = KeyboardButtonColor.Default,
            Extra = "UnbanRequestDelete",
            Type = "command",
            Label = "Очистить"
        });
        if (requests.Count <= 1)
        {
            keyboard.AddLine();
            keyboard.AddButton(new AddButtonParams
            {
                ActionType = KeyboardButtonActionType.Text,
                Color = KeyboardButtonColor.Negative,
                Extra = "AdminPanel",
                Type = "command",
                Label = "Вернуться назад"
            });
        }
        else
        {
            keyboard.AddLine();
            if (index > 0)
                keyboard.AddButton(new AddButtonParams
                {
                    ActionType = KeyboardButtonActionType.Text,
                    Color = KeyboardButtonColor.Primary,
                    Extra = "UnbanRequestMoveBack",
                    Type = "command",
                    Label = "⏪"
                });
            if (index + 1 < requests.Count)
                keyboard.AddButton(new AddButtonParams
                {
                    ActionType = KeyboardButtonActionType.Text,
                    Color = KeyboardButtonColor.Primary,
                    Extra = "UnbanRequestMoveForward",
                    Type = "command",
                    Label = "⏩"
                });
            keyboard.AddLine();
            keyboard.AddButton(new AddButtonParams
            {
                ActionType = KeyboardButtonActionType.Text,
                Color = KeyboardButtonColor.Negative,
                Extra = "AdminPanel",
                Type = "command",
                Label = "Вернуться назад"
            });
        }

        return JsonConvert.SerializeObject(keyboard.Build());
    }

    public void PanelUnbanRequests(long user)
    {
        var requests = CacheFramework.BannedRequestsFull.Where(x => !x.IsWatched).ToList();
        if (!requests.Any())
        {
            _vkController.SendMessage(_phraseFramework.NoBannedRequests, GoToAdminPanel, user);
            return;
        }

        _status.SetAdminCurrentUnbanRequestPageIndex(user, 0);
        var keyboard = GetUnbanRequestKeyboard(user);

        _vkController.SendMessage(GetUnbanRequestOverview(user), keyboard, user);
    }
}
