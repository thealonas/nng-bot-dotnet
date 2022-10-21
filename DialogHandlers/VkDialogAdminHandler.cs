using System.Text;
using nng_bot.API;
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
        var keyboard = AdminPanelButtons;
        var returnKeyboard = GoToAdminPanel;

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

        var profileTarget = (int) Math.Abs(firstElement);

        if (profileTarget != 0)
        {
            var targetProfile = _cacheFramework.LoadProfile(profileTarget);
            _vkController.SendMessage(_phraseFramework.FormProfile(targetProfile, IfUserPrioritized(profileTarget)),
                GoToAdminPanel, user);
        }
        else
        {
            _vkController.SendMessage(_phraseFramework.UserIdIsNotInteger,
                returnKeyboard, user);
            _status.AdminRequests.Remove(adminRequest);
        }

        _status.AdminRequests.Remove(adminRequest);
    }

    public void PanelEnter(long user)
    {
        _vkController.SendMessage(_phraseFramework.AdminPanel, AdminPanelButtons, user);
    }

    public void ShowUserProfile(long user)
    {
        _status.AddAdminRequest(new AdminRequest(user));
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

    public void ClearBanned(long user)
    {
        if (!_cacheScheduledTaskProcessor.IsInstantUpdateAvailable)
        {
            _vkController.SendMessage(_phraseFramework.CacheIsAboutToUpdateItself,
                GoToAdminPanel, user);
            return;
        }

        _vkController.SendMessage(_phraseFramework.DataUpdatingStarted, GoToMenuButtons,
            user);

        _cacheScheduledTaskProcessor.ForceUpdateData();
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
}
