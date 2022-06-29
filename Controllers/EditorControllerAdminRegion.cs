using System.Text;
using Newtonsoft.Json;
using nng_bot.Enums;
using nng_bot.Extensions;
using nng_bot.Frameworks;
using nng_bot.Models;
using VkNet.Enums.SafetyEnums;
using VkNet.Exception;
using VkNet.Model;
using VkNet.Model.Keyboard;
using static nng_bot.API.KeyBoardFramework;
using static nng_bot.Configs.UsersConfigurationProcessor;

namespace nng_bot.Controllers;

public partial class EditorController
{
    #region AdminFunctions

    private void SendErrorMessage(long user, string error = "")
    {
        VkController.SendMessage(PhraseFramework.Error(error),
            GoToMenuButtons, user);
    }

    private void ProcessAdminRequestMain(long dialog, long user, Message message)
    {
        var adminRequest = Status.AdminRequests.First(x => x.Admin == user);

        var keyboard =
            adminRequest.Type is AdminRequestType.MakeLimitless or AdminRequestType.EditEditorRestrictions
                ? AdminLimitsButtons
                : AdminPanelButtons;

        var returnKeyboard =
            adminRequest.Type is AdminRequestType.MakeLimitless or AdminRequestType.EditEditorRestrictions
                ? GoToAdminLimitPanel
                : GoToAdminPanel;

        if (message.Payload?.Length > 0)
        {
            VkController.SendMessage(PhraseFramework.AdminPanel,
                keyboard, dialog);
            Status.AdminRequests.Remove(adminRequest);
            return;
        }

        var ids = message.Text.Trim().ToLower().Split(',').ToList();
        ids = ids.Select(x => x.Replace("id", string.Empty)
            .Replace("https://vk.com/", string.Empty)).ToList();

        var users = new List<long>();

        foreach (var id in ids)
        {
            if (!long.TryParse(id, out var userId)) continue;
            users.Add(userId);
        }

        if (!users.Any())
        {
            VkController.SendMessage(PhraseFramework.UserIdIsNotInteger,
                returnKeyboard, dialog);
            Status.AdminRequests.Remove(adminRequest);
            return;
        }

        var firstElement = users.First();

        switch (adminRequest.Type)
        {
            case AdminRequestType.EditEditorRestrictions:
                var config = UsersConfiguration;
                var target = (int) Math.Abs(firstElement);

                if (target == 0)
                {
                    VkController.SendMessage(PhraseFramework.UserIdIsNotInteger,
                        returnKeyboard, dialog);
                    Status.AdminRequests.Remove(adminRequest);
                    return;
                }

                config.EditorRestriction = target;
                SaveUsersConfig(config);
                VkController.SendMessage(PhraseFramework.EditorRestrictionChanged(firstElement),
                    GoToAdminLimitPanel, dialog);
                break;

            case AdminRequestType.ShowOtherUserProfile:
                var profileTarget = (int) Math.Abs(firstElement);

                if (profileTarget == 0)
                {
                    VkController.SendMessage(PhraseFramework.UserIdIsNotInteger,
                        returnKeyboard, dialog);
                    Status.AdminRequests.Remove(adminRequest);
                    break;
                }

                var targetProfile = CacheFramework.LoadProfile(profileTarget);
                VkController.SendMessage(PhraseFramework.FormProfile(targetProfile, IfUserPrioritized(profileTarget)),
                    GoToAdminPanel, dialog);
                break;

            default:
                ProcessAdminRequest(adminRequest, dialog, users, returnKeyboard);
                break;
        }

        Status.AdminRequests.Remove(adminRequest);
    }

    #region UnbanRequestProcessors

    private void ProcessUnbanRequestDelete(long user)
    {
        var requests = CacheFramework.BannedRequestsFull.ToList();
        var count = requests.RemoveAll(x => x.Status is UnbanRequestStatus.Accepted or UnbanRequestStatus.Rejected
                                            || x.IsWatched || !CacheFramework.IsBanned(x.UserId));
        requests = requests.ToList();
        CacheFramework.SaveBannedRequests(requests);
        VkController.SendMessage(PhraseFramework.DeletedUnbanRequests($"{count}/{requests.Count + count}"),
            GoToAdminUnbanRequests, user);
    }

    private string GetUnbanRequestOverview(long adminId)
    {
        var index = Status.GetAdminCurrentUnbanRequestPageIndex(adminId);
        var requests = CacheFramework.BannedRequestsFull.Where(x => !x.IsWatched).ToList();

        var targetRequest = GetUnbanRequest(adminId, index);

        var phrase = PhraseFramework.UnbanRequestAdminOverview($"{index + 1}/{requests.Count}",
            targetRequest.Status.GetName(), targetRequest.CreatedOn.ToString("dd.MM.yyyy HH:mm:ss"),
            PhraseFramework.FormProfile(CacheFramework.LoadProfile(targetRequest.UserId), false));
        return phrase;
    }

    private UnbanRequest GetUnbanRequest(long user, int index)
    {
        var unbanRequests = CacheFramework.BannedRequestsFull.Where(x => !x.IsWatched).ToList();
        UnbanRequest targetRequest;
        try
        {
            targetRequest = unbanRequests[index];
        }
        catch (ArgumentOutOfRangeException)
        {
            Logger.LogCritical(
                "Попытка получить запрос на разбан пользователя с индексом {Index} в коллекции запросов на разбан пользователей произошла в методе {Method}",
                index, nameof(GetUnbanRequest));
            Status.SetAdminCurrentUnbanRequestPageIndex(user, 0);
            targetRequest = unbanRequests[0];
        }

        return targetRequest;
    }

    private string GetUnbanRequestKeyboard(long adminId)
    {
        var index = Status.GetAdminCurrentUnbanRequestPageIndex(adminId);
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

    #endregion

    #region UnbanRequestMovement

    private void ProcessAdminPanelUnbanRequestMoveForward(long user)
    {
        var index = Status.GetAdminCurrentUnbanRequestPageIndex(user) + 1;
        Status.SetAdminCurrentUnbanRequestPageIndex(user, index);

        var phrase = GetUnbanRequestOverview(user);
        var keyboard = GetUnbanRequestKeyboard(user);

        VkController.SendMessage(phrase, keyboard, user);
    }

    private void ProcessAdminPanelUnbanRequestMoveBackward(long user)
    {
        var index = Status.GetAdminCurrentUnbanRequestPageIndex(user) - 1;
        Status.SetAdminCurrentUnbanRequestPageIndex(user, index);

        var phrase = GetUnbanRequestOverview(user);
        var keyboard = GetUnbanRequestKeyboard(user);

        VkController.SendMessage(phrase, keyboard, user);
    }

    private void ProcessAdminUnbanRequestAccept(long user)
    {
        var index = Status.GetAdminCurrentUnbanRequestPageIndex(user);
        var targetRequest = GetUnbanRequest(user, index);

        CacheFramework.SetBannedRequestStatus(targetRequest, UnbanRequestStatus.Accepted);
        CacheFramework.SetBannedWatchedStatus(targetRequest, true);

        Status.UnbannedUsers.Add(targetRequest.UserId);
        ProcessUnbanUserInAllGroups(user, targetRequest);
    }

    private void ProcessUnbanUserInAllGroups(long admin, UnbanRequest request)
    {
        var data = CacheFramework.LoadCache().Data;
        var userObject = new User
        {
            Id = request.UserId
        };
        foreach (var cacheGroup in data)
        {
            var banned = VkFramework.GetBanned(cacheGroup.Id);
            if (banned.All(x => x.Id != request.UserId)) continue;
            var group = new Group
            {
                Id = cacheGroup.Id
            };
            try
            {
                VkFramework.UnBlock(group, userObject);
            }
            catch (VkApiException e)
            {
                Logger.LogError("Невозможно разбанить пользователя в группе {Group}: {Type}: {Message}",
                    cacheGroup.Id, e.GetType(), e.Message);
            }
        }

        VkController.SendMessage(PhraseFramework.YouHaveBeenUnbanned,
            null, request.UserId);
        VkController.SendMessage(PhraseFramework.UserHasBeenUnbanned(request.UserId),
            AdminPanelButtons, admin);
    }

    private void ProcessAdminUnbanRequestDeny(long user)
    {
        var index = Status.GetAdminCurrentUnbanRequestPageIndex(user);
        var targetRequest = GetUnbanRequest(user, index);

        CacheFramework.SetBannedRequestStatus(targetRequest, UnbanRequestStatus.Rejected);
        CacheFramework.SetBannedWatchedStatus(targetRequest, true);

        Status.UnbannedUsers.RemoveIfExists(targetRequest.UserId);

        VkController.SendMessage(PhraseFramework.YourRequestHasBeenRejected,
            null, targetRequest.UserId);
        VkController.SendMessage(PhraseFramework.UserRequestHasBeenRejected(targetRequest.UserId),
            AdminPanelButtons, user);
    }

    #endregion

    private void ProcessAdminPanelUnbanRequests(long user)
    {
        var requests = CacheFramework.BannedRequestsFull.Where(x => !x.IsWatched).ToList();
        if (!requests.Any())
        {
            VkController.SendMessage(PhraseFramework.NoBannedRequests, GoToAdminPanel, user);
            return;
        }

        Status.SetAdminCurrentUnbanRequestPageIndex(user, 0);
        var keyboard = GetUnbanRequestKeyboard(user);

        VkController.SendMessage(GetUnbanRequestOverview(user), keyboard, user);
    }

    private void ProcessAdminPanelEnter(long user)
    {
        VkController.SendMessage(PhraseFramework.AdminPanel, AdminPanelButtons, user);
    }

    private void ProcessAdminBanUser(long user)
    {
        var current = UsersConfiguration.BannedUsers;
        Status.AddAdminRequest(new AdminRequest(user, AdminRequestType.BanUser));
        VkController.SendMessage(PhraseFramework.WriteDownBanUserId(string.Join(", ", current)), GoToAdminPanel,
            user);
    }

    private void ProcessAdminMakeAdmin(long user)
    {
        var current = UsersConfiguration.AdminUsers;
        Status.AddAdminRequest(new AdminRequest(user, AdminRequestType.MakeAdmin));
        VkController.SendMessage(PhraseFramework.WriteDownAdminUserId(string.Join(", ", current)),
            GoToAdminPanel, user);
    }

    private void ProcessAdminMakeLimitless(long user)
    {
        var current = UsersConfiguration.PriorityUsers;
        Status.AddAdminRequest(new AdminRequest(user, AdminRequestType.MakeLimitless));
        VkController.SendMessage(PhraseFramework.WriteDownLimitlessUserId(string.Join(", ", current)),
            GoToAdminUnbanRequests, user);
    }

    private void ProcessAdminShowOtherUserProfile(long user)
    {
        Status.AddAdminRequest(new AdminRequest(user, AdminRequestType.ShowOtherUserProfile));
        VkController.SendMessage(PhraseFramework.WriteDownUserIdToShowHisProfile,
            GoToAdminPanel, user);
    }

    private void ProcessAdminClearCache(long user)
    {
        if (!CacheScheduledTaskProcessor.IsInstantUpdateAvailable)
        {
            VkController.SendMessage(PhraseFramework.CacheIsAboutToUpdateItself,
                GoToAdminPanel, user);
            return;
        }

        CacheScheduledTaskProcessor.ForceUpdateCache();

        VkController.SendMessage(PhraseFramework.CacheCleaningStarted, GoToMenuButtons,
            user);
    }

    private void ProcessAdminGetStatistics(long user)
    {
        var cache = CacheFramework.LoadCache();
        var list = new CacheObjectList(cache.Data.Select(group => new CacheObject(group.Id)).ToList());
        var celling = UsersConfiguration.GroupManagersCeiling;

        var phrase = PhraseFramework.CacheOutput(cache.Data.Count, $"{list.TotalBusySlots}/{list.TotalSlots}",
            $"{list.TotalMembers}", list.TotalMembersWithoutDuplicates, list.TotalManagers,
            list.TotalManagersWithoutDuplicates);

        var groups = new StringBuilder();
        foreach (var group in cache.Data)
        {
            var canGrantEditors = group.ManagerTotalCount >= celling ? "✘" : "✔";
            groups.AppendLine(
                $"{canGrantEditors} @{group.ShortName} — редакторы: {group.ManagerTotalCount}/{celling} — участники: {group.MembersTotalCount}");
        }

        VkController.SendMessage(phrase, null, user);
        Task.Delay(300).ContinueWith(_ => VkController.SendMessage(groups.ToString(), GoToAdminPanel, user))
            .GetAwaiter().GetResult();
    }

    private string ProcessAdminRequestAdd(in AdminRequest adminRequest, long victim)
    {
        var config = UsersConfiguration;
        string response;
        switch (adminRequest.Type)
        {
            case AdminRequestType.BanUser:
                if (!config.BannedUsers.Contains(victim))
                {
                    config.BannedUsers.Add(victim);

                    response = PhraseFramework.UserBannedSuccessfully(victim);

                    SaveUsersConfig(config);
                    Status.AdminRequests.Remove(adminRequest);
                }
                else
                {
                    response = PhraseFramework.UserAlreadyBanned(victim);
                }

                break;

            case AdminRequestType.MakeAdmin:
                if (!config.AdminUsers.Contains(victim))
                {
                    config.AdminUsers.Add(victim);

                    response = PhraseFramework.UserMadeAdminSuccessfully(victim);

                    SaveUsersConfig(config);
                }
                else
                {
                    response = PhraseFramework.UserAlreadyAdmin(victim);
                }

                break;

            case AdminRequestType.MakeLimitless:
                if (!config.PriorityUsers.Contains(victim))
                {
                    config.PriorityUsers.Add(victim);

                    response = PhraseFramework.UserMadeLimitlessSuccessfully(victim);

                    SaveUsersConfig(config);
                }
                else
                {
                    response = PhraseFramework.UserAlreadyLimitless(victim);
                }

                break;

            default:
                throw new InvalidOperationException($"Отсутсвует реализация для типа: {adminRequest.Type}");
        }

        return response;
    }

    private string ProcessAdminRequestRemove(in AdminRequest adminRequest, long victim)
    {
        var config = UsersConfiguration;
        string response;
        switch (adminRequest.Type)
        {
            case AdminRequestType.BanUser:
                if (config.BannedUsers.Contains(victim))
                {
                    config.BannedUsers.Remove(victim);
                    response = PhraseFramework.UserUnbannedSuccessfully(victim);
                    SaveUsersConfig(config);
                    Status.AdminRequests.Remove(adminRequest);
                }
                else
                {
                    response = PhraseFramework.UserIsNotBanned(victim);
                }

                break;

            case AdminRequestType.MakeAdmin:
                if (config.AdminUsers.Contains(victim))
                {
                    config.AdminUsers.Remove(victim);

                    response = PhraseFramework.UserRemovedFromAdminSuccessfully(victim);

                    SaveUsersConfig(config);
                }
                else
                {
                    response = PhraseFramework.UserIsNotAdmin(victim);
                }

                break;

            case AdminRequestType.MakeLimitless:
                if (config.PriorityUsers.Contains(victim))
                {
                    config.PriorityUsers.Remove(victim);

                    response = PhraseFramework.UserRemovedFromLimitlessSuccessfully(victim);

                    SaveUsersConfig(config);
                }
                else
                {
                    response = PhraseFramework.UserIsNotLimitless(victim);
                }

                break;

            default:
                throw new InvalidOperationException($"Отсутсвует реализация для типа: {adminRequest.Type}");
        }

        return response;
    }

    private void ProcessAdminEditEditorRestrictions(long user)
    {
        var current = UsersConfiguration.EditorRestriction;
        Status.AddAdminRequest(new AdminRequest(user, AdminRequestType.EditEditorRestrictions));
        VkController.SendMessage(PhraseFramework.WriteDownNewRestriction(current), GoToAdminLimitPanel, user);
    }

    private void ProcessAdminRequest(AdminRequest adminRequest, long admin, IEnumerable<long> users,
        string? returnKeyboard)
    {
        var phrase = new StringBuilder();
        foreach (var user in users)
        {
            if (user == 0)
            {
                phrase.Append("🙄 Нуль не поддерживается\n");
                continue;
            }

            phrase.Append(user < 0
                ? ProcessAdminRequestRemove(adminRequest, -user)
                : ProcessAdminRequestAdd(adminRequest, user));
            phrase.Append('\n');
        }

        VkController.SendMessage(phrase.ToString(), returnKeyboard, admin);
    }

    #endregion
}
