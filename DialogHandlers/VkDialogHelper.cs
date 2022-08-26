using nng_bot.API;
using nng_bot.Exceptions;
using nng_bot.Extensions;
using nng_bot.Frameworks;
using nng_bot.Models;
using nng.Enums;
using static nng_bot.Configs.UsersConfigurationProcessor;
using static nng_bot.API.KeyBoardFramework;

namespace nng;

public class VkDialogHelper
{
    private readonly CacheFramework _cacheFramework;
    private readonly PhraseFramework _phraseFramework;

    private readonly OperationStatus _status;
    private readonly VkController _vkController;

    public VkDialogHelper(OperationStatus status, VkController vkController, PhraseFramework phraseFramework,
        CacheFramework cacheFramework)
    {
        _status = status;
        _vkController = vkController;
        _phraseFramework = phraseFramework;
        _cacheFramework = cacheFramework;
    }

    public bool CheckIfBanned(UserProfile profile)
    {
        return (profile.Banned && !profile.Deleted) || IfUserIsBannedInBot(profile.Id);
    }

    public void SendErrorMessage(long user, string error = "")
    {
        _vkController.SendMessage(_phraseFramework.Error(error),
            GoToMenuButtons, user);
    }

    public bool CheckIfCooldown(long user)
    {
        if (_status.CoolDownUsers.All(x => user != x) && !IfLimitlessUserIsOnCoolDown(user)) return false;

        _vkController.SendMessage(_phraseFramework.YouAreOnCoolDown,
            GoToMenuButtons, user);

        return true;
    }

    public CacheGroup ChooseGroup(long userId, bool chooseFirst = true)
    {
        var cache = CacheFramework.LoadCache();
        var groups = Enumerable.Reverse(cache.Data).ToList();
        var priority = IfUserPrioritized(userId);
        var managersCelling = ManagersCeiling;

        CacheGroup potentialGroup;
        try
        {
            if (chooseFirst)
            {
                potentialGroup = groups.First();

                if (potentialGroup.Members is {Count: < 50} && !priority && !potentialGroup.IsManager(userId))
                    throw new LessThanFiftySubs(potentialGroup);
                if (potentialGroup.Members is {Count: >= 50} && potentialGroup.Members.Count < managersCelling &&
                    !priority && !potentialGroup.IsManager(userId))
                    return potentialGroup;
            }

            try
            {
                potentialGroup = groups.Where(cacheGroup =>
                    cacheGroup.Members.Count >= 50 && !cacheGroup.IsManager(userId) &&
                    cacheGroup.ManagerTotalCount < managersCelling).GetRandom();
            }
            catch (Exception)
            {
                throw new InvalidOperationException();
            }
        }
        catch (InvalidOperationException)
        {
            try
            {
                potentialGroup =
                    groups.First(
                        x => x.Managers.All(manager => manager != userId) && x.Managers.Count < managersCelling);
                if (potentialGroup.Members.Count < 50)
                    throw new LessThanFiftySubs(potentialGroup);
            }
            catch (InvalidOperationException)
            {
                throw new NoAvailableGroups();
            }
        }

        return potentialGroup;
    }

    public bool IfLimitlessUserIsOnCoolDown(long user)
    {
        if (!_status.LimitlessLimit.ContainsKey(user)) return false;
        return (DateTime.Now - _status.LimitlessLimit[user]).TotalMinutes < 15;
    }

    public void ReplaceLongToGroup(ref CacheData cachedGroups, EditorRequest request)
    {
        for (var i = 0; i < cachedGroups.Data.Count; i++)
        {
            if (cachedGroups.Data[i].Id != request.Group) continue;
            cachedGroups.Data[i] = _vkController.GetGroupInfo(request.Group);
            return;
        }
    }

    public string GetBannedKeyboard(BanPriority priority)
    {
        return priority is not BanPriority.Red and not BanPriority.Green and not BanPriority.White
            ? RestrictedStartButtons
            : RestrictedStartButtonsWithUnbanRequest;
    }

    public string GetStartMenuKeyboard(long id)
    {
        if (_cacheFramework.TryGetBannedPriority(id, out var priority)) return GetBannedKeyboard(priority);
        return IfUserIsAdmin(id) ? AdminStartButtons : StartButtons;
    }
}
