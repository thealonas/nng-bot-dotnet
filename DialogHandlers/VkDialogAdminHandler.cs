using System.Text;
using nng_bot.API;
using nng_bot.Frameworks;
using nng_bot.Models;
using nng.VkFrameworks;
using VkNet.Enums;
using VkNet.Exception;
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

    private bool TryGetUserId(string id, out long userId)
    {
        userId = 0;
        
        if (string.IsNullOrEmpty(id)) return false;
        if (long.TryParse(id, out userId) && userId != 0) return true;

        var targetScreenName = id.Trim().ToLower()
            .Replace("http://", string.Empty)
            .Replace("https://", string.Empty)
            .Replace("vk.com/", string.Empty);

        VkObject user;

        try
        {
            user = VkFrameworkExecution.ExecuteWithReturn(() =>
                _vkController.GroupFramework.Utils.ResolveScreenName(targetScreenName));
        }
        catch (ParameterMissingOrInvalidException)
        {
            return false;
        }

        if (user?.Type is not VkObjectType.User || user.Id is null) return false;

        userId = user.Id.Value;
        return true;
    }

    public void SendProfileInfo(long user, Message message, AdminRequest adminRequest)
    {
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

        if (!ids.Any() || !TryGetUserId(ids.First(), out var outputUser))
        {
            _vkController.SendMessage(_phraseFramework.CannotFindUserId, returnKeyboard, user);
            return;
        }

        var targetProfile = _cacheFramework.LoadProfile(outputUser);
        _vkController.SendMessage(_phraseFramework.FormProfile(targetProfile, IfUserPrioritized(outputUser)),
            GoToAdminPanel, user);
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
