using System.Text;
using nng_bot.API;
using nng_bot.Enums;
using nng_bot.Frameworks;
using nng_bot.Models;
using VkNet.Model;
using static nng_bot.Configs.UsersConfigurationProcessor;
using static nng_bot.API.KeyBoardFramework;

namespace nng;

public class VkDialogAdminHandler
{
    private readonly CacheFramework _cacheFramework;
    private readonly CacheScheduledTaskProcessor _cacheScheduledTaskProcessor;
    private readonly PhraseFramework _phraseFramework;

    private readonly OperationStatus _status;
    private readonly VkController _vkController;

    public VkDialogAdminHandler(OperationStatus status, VkController vkController, PhraseFramework phraseFramework,
        CacheFramework cacheFramework, CacheScheduledTaskProcessor cacheScheduledTaskProcessor)
    {
        _status = status;
        _vkController = vkController;
        _phraseFramework = phraseFramework;
        _cacheFramework = cacheFramework;
        _cacheScheduledTaskProcessor = cacheScheduledTaskProcessor;
    }

    public void AdminRequestMain(long user, Message message)
    {
        var adminRequest = _status.AdminRequests.First(x => x.Admin == user);

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
            _vkController.SendMessage(_phraseFramework.AdminPanel,
                keyboard, user);
            _status.AdminRequests.Remove(adminRequest);
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
            _vkController.SendMessage(_phraseFramework.UserIdIsNotInteger,
                returnKeyboard, user);
            _status.AdminRequests.Remove(adminRequest);
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
                    _vkController.SendMessage(_phraseFramework.UserIdIsNotInteger,
                        returnKeyboard, user);
                    _status.AdminRequests.Remove(adminRequest);
                    return;
                }

                config.EditorRestriction = target;
                SaveUsersConfig(config);
                _vkController.SendMessage(_phraseFramework.EditorRestrictionChanged(firstElement),
                    GoToAdminLimitPanel, user);
                break;

            case AdminRequestType.ShowOtherUserProfile:
                var profileTarget = (int) Math.Abs(firstElement);

                if (profileTarget == 0)
                {
                    _vkController.SendMessage(_phraseFramework.UserIdIsNotInteger,
                        returnKeyboard, user);
                    _status.AdminRequests.Remove(adminRequest);
                    break;
                }

                var targetProfile = _cacheFramework.LoadProfile(profileTarget);
                _vkController.SendMessage(_phraseFramework.FormProfile(targetProfile, IfUserPrioritized(profileTarget)),
                    GoToAdminPanel, user);
                break;

            default:
                AdminRequest(adminRequest, user, users, returnKeyboard);
                break;
        }

        _status.AdminRequests.Remove(adminRequest);
    }

    private void AdminRequest(AdminRequest adminRequest, long admin, IEnumerable<long> users,
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
                ? RequestRemove(adminRequest, -user)
                : AdminRequestAdd(adminRequest, user));
            phrase.Append('\n');
        }

        _vkController.SendMessage(phrase.ToString(), returnKeyboard, admin);
    }

    public void PanelEnter(long user)
    {
        _vkController.SendMessage(_phraseFramework.AdminPanel, AdminPanelButtons, user);
    }

    public void BanUser(long user)
    {
        var current = UsersConfiguration.BannedUsers;
        _status.AddAdminRequest(new AdminRequest(user, AdminRequestType.BanUser));
        _vkController.SendMessage(_phraseFramework.WriteDownBanUserId(string.Join(", ", current)), GoToAdminPanel,
            user);
    }

    public void MakeAdmin(long user)
    {
        var current = UsersConfiguration.AdminUsers;
        _status.AddAdminRequest(new AdminRequest(user, AdminRequestType.MakeAdmin));
        _vkController.SendMessage(_phraseFramework.WriteDownAdminUserId(string.Join(", ", current)),
            GoToAdminPanel, user);
    }

    public void MakeLimitless(long user)
    {
        var current = UsersConfiguration.PriorityUsers;
        _status.AddAdminRequest(new AdminRequest(user, AdminRequestType.MakeLimitless));
        _vkController.SendMessage(_phraseFramework.WriteDownLimitlessUserId(string.Join(", ", current)),
            GoToAdminUnbanRequests, user);
    }

    public void ShowUserProfile(long user)
    {
        _status.AddAdminRequest(new AdminRequest(user, AdminRequestType.ShowOtherUserProfile));
        _vkController.SendMessage(_phraseFramework.WriteDownUserIdToShowHisProfile,
            GoToAdminPanel, user);
    }

    public void ClearCache(long user)
    {
        if (!_cacheScheduledTaskProcessor.IsInstantUpdateAvailable)
        {
            _vkController.SendMessage(_phraseFramework.CacheIsAboutToUpdateItself,
                GoToAdminPanel, user);
            return;
        }

        _vkController.SendMessage(_phraseFramework.CacheCleaningStarted, GoToMenuButtons,
            user);

        _cacheScheduledTaskProcessor.ForceUpdateCache();
    }

    public void GetStatistics(long user)
    {
        var cache = CacheFramework.LoadCache();
        var list = new CacheObjectList(cache.Data.Select(group => new CacheObject(group.Id)).ToList());
        var celling = UsersConfiguration.GroupManagersCeiling;

        var phrase = _phraseFramework.CacheOutput(cache.Data.Count, $"{list.TotalBusySlots}/{list.TotalSlots}",
            $"{list.TotalMembers}", list.TotalMembersWithoutDuplicates, list.TotalManagers,
            list.TotalManagersWithoutDuplicates);

        var groups = new StringBuilder();
        foreach (var group in cache.Data)
        {
            var canGrantEditors = group.ManagerTotalCount >= celling ? "✘" : "✔";
            groups.AppendLine(
                $"{canGrantEditors} @{group.ShortName} — редакторы: {group.ManagerTotalCount}/{celling} — участники: {group.MembersTotalCount}");
        }

        _vkController.SendMessage(phrase, null, user);
        Task.Delay(300).ContinueWith(_ => _vkController.SendMessage(groups.ToString(), GoToAdminPanel, user))
            .GetAwaiter().GetResult();
    }

    private string AdminRequestAdd(in AdminRequest adminRequest, long victim)
    {
        var config = UsersConfiguration;
        string response;
        switch (adminRequest.Type)
        {
            case AdminRequestType.BanUser:
                if (!config.BannedUsers.Contains(victim))
                {
                    config.BannedUsers.Add(victim);

                    response = _phraseFramework.UserBannedSuccessfully(victim);

                    SaveUsersConfig(config);
                    _status.AdminRequests.Remove(adminRequest);
                }
                else
                {
                    response = _phraseFramework.UserAlreadyBanned(victim);
                }

                break;

            case AdminRequestType.MakeAdmin:
                if (!config.AdminUsers.Contains(victim))
                {
                    config.AdminUsers.Add(victim);

                    response = _phraseFramework.UserMadeAdminSuccessfully(victim);

                    SaveUsersConfig(config);
                }
                else
                {
                    response = _phraseFramework.UserAlreadyAdmin(victim);
                }

                break;

            case AdminRequestType.MakeLimitless:
                if (!config.PriorityUsers.Contains(victim))
                {
                    config.PriorityUsers.Add(victim);

                    response = _phraseFramework.UserMadeLimitlessSuccessfully(victim);

                    SaveUsersConfig(config);
                }
                else
                {
                    response = _phraseFramework.UserAlreadyLimitless(victim);
                }

                break;

            default:
                throw new InvalidOperationException($"Отсутсвует реализация для типа: {adminRequest.Type}");
        }

        return response;
    }

    private string RequestRemove(in AdminRequest adminRequest, long victim)
    {
        var config = UsersConfiguration;
        string response;
        switch (adminRequest.Type)
        {
            case AdminRequestType.BanUser:
                if (config.BannedUsers.Contains(victim))
                {
                    config.BannedUsers.Remove(victim);
                    response = _phraseFramework.UserUnbannedSuccessfully(victim);
                    SaveUsersConfig(config);
                    _status.AdminRequests.Remove(adminRequest);
                }
                else
                {
                    response = _phraseFramework.UserIsNotBanned(victim);
                }

                break;

            case AdminRequestType.MakeAdmin:
                if (config.AdminUsers.Contains(victim))
                {
                    config.AdminUsers.Remove(victim);

                    response = _phraseFramework.UserRemovedFromAdminSuccessfully(victim);

                    SaveUsersConfig(config);
                }
                else
                {
                    response = _phraseFramework.UserIsNotAdmin(victim);
                }

                break;

            case AdminRequestType.MakeLimitless:
                if (config.PriorityUsers.Contains(victim))
                {
                    config.PriorityUsers.Remove(victim);

                    response = _phraseFramework.UserRemovedFromLimitlessSuccessfully(victim);

                    SaveUsersConfig(config);
                }
                else
                {
                    response = _phraseFramework.UserIsNotLimitless(victim);
                }

                break;

            default:
                throw new InvalidOperationException($"Отсутсвует реализация для типа: {adminRequest.Type}");
        }

        return response;
    }

    public void EditEditorRestrictions(long user)
    {
        var current = UsersConfiguration.EditorRestriction;
        _status.AddAdminRequest(new AdminRequest(user, AdminRequestType.EditEditorRestrictions));
        _vkController.SendMessage(_phraseFramework.WriteDownNewRestriction(current), GoToAdminLimitPanel, user);
    }
}
