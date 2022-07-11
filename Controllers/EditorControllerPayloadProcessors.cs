using nng.Enums;
using nng_bot.Exceptions;
using nng_bot.Extensions;
using nng_bot.Frameworks;
using nng_bot.Models;
using Sentry;
using VkNet.Enums.SafetyEnums;
using VkNet.Exception;
using static nng_bot.API.KeyBoardFramework;
using static nng_bot.Configs.UsersConfigurationProcessor;

namespace nng_bot.Controllers;

public partial class EditorController
{
    private static bool CheckIfBanned(UserProfile profile)
    {
        return (profile.Banned && !profile.Deleted) || IfUserIsBannedInBot(profile.Id);
    }

    private bool CheckIfCooldown(long user)
    {
        if (Status.CoolDownUsers.All(x => user != x) && !IfLimitlessUserIsOnCoolDown(user)) return false;

        VkController.SendMessage(PhraseFramework.YouAreOnCoolDown,
            GoToMenuButtons, user);

        return true;
    }

    private static CacheGroup ChooseGroup(long userId, bool chooseFirst = true)
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


    private bool IfLimitlessUserIsOnCoolDown(long user)
    {
        if (!Status.LimitlessLimit.ContainsKey(user)) return false;
        return (DateTime.Now - Status.LimitlessLimit[user]).TotalMinutes < 15;
    }

    private void ReplaceLongToGroup(ref CacheData cachedGroups, EditorRequest request)
    {
        for (var i = 0; i < cachedGroups.Data.Count; i++)
        {
            if (cachedGroups.Data[i].Id != request.Group) continue;
            cachedGroups.Data[i] = VkController.GetGroupInfo(request.Group);
            return;
        }
    }

    private void ProcessGiveEditor(long user)
    {
        var userProfile = CacheFramework.LoadProfile(user);
        var priority = IfUserPrioritized(user);

        if (userProfile.CreatedOn != null && (DateTime.Now - userProfile.CreatedOn.Value).TotalDays < 180)
        {
            VkController.SendMessage(PhraseFramework.YourAccountIsTooYoung,
                GoToMenuButtons, user);
            return;
        }

        if (CheckIfCooldown(user)) return;

        if (CheckIfBanned(userProfile))
        {
            VkController.SendMessage(PhraseFramework.TempUnavailable,
                GoToMenuButtons, user);
            return;
        }

        if (userProfile.EditorGroups.Length >= EditorRestrictions && !priority)
        {
            VkController.SendMessage(PhraseFramework.LimitReached,
                GoToMenuButtons, user);
            return;
        }

        CacheGroup group;

        try
        {
            group = ChooseGroup(user, !priority);
        }
        catch (LessThanFiftySubs lessThanFiftySubs)
        {
            var actualGroup = VkController.GetGroupInfo(lessThanFiftySubs.Group.Id);
            if (actualGroup.Members.Contains(user))
            {
                VkController.SendMessage(PhraseFramework.NoAvailableSlots,
                    GoToMenuButtons, user);
                return;
            }

            VkController.SendMessage(PhraseFramework.LessThanFiftySubs(lessThanFiftySubs.Group),
                IveJoinedButtonsLessThanFiftySubs, user);
            return;
        }
        catch (NoAvailableGroups)
        {
            VkController.SendMessage(PhraseFramework.NoAvailableSlots,
                GoToMenuButtons, user);
            return;
        }

        Status.AddUserAskedForEditor(user);
        if (Status.UsersAskedForEditor[user] > 3)
        {
            VkController.SendMessage(PhraseFramework.YouAreOnCoolDown,
                GoToMenuButtons, user);
            return;
        }

        Status.AddEditorRequest(new EditorRequest(user, group.Id));
        VkController.SendMessage(PhraseFramework.PleaseJoinGroup(group), IveJoinedButtons, user);
    }

    private async Task ProcessIveJoined(long user)
    {
        var cachedGroups = CacheFramework.LoadCache();
        var priority = IfUserPrioritized(user);
        var joinedProfile = CacheFramework.LoadProfile(user);

        EditorRequest request;
        try
        {
            request = Status.UsersToEditorGiving.First(x => x.User == user);
        }
        catch (InvalidOperationException)
        {
            VkController.SendMessage(PhraseFramework.YourRequestNoLongerValid,
                GoToMenuButtons, user);
            return;
        }
        catch (Exception e)
        {
            SentrySdk.CaptureException(e);
            var logToShow = $"{e.GetType()}: {e.Message}";
            VkController.SendMessage(PhraseFramework.Error(logToShow),
                GoToMenuButtons, user);
            return;
        }

        if (CheckIfCooldown(user))
        {
            Status.UsersToEditorGiving.Remove(request);
            return;
        }

        if (joinedProfile.CreatedOn != null && (DateTime.Now - joinedProfile.CreatedOn.Value).TotalDays < 180)
        {
            VkController.SendMessage(PhraseFramework.YourAccountIsTooYoung,
                GoToMenuButtons, user);
            Status.UsersToEditorGiving.Remove(request);
            return;
        }

        if (CheckIfBanned(joinedProfile))
        {
            VkController.SendMessage(PhraseFramework.TempUnavailable,
                GoToMenuButtons, user);
            Status.UsersToEditorGiving.Remove(request);
            return;
        }

        if (joinedProfile.EditorGroups.Length >= EditorRestrictions && !priority)
        {
            VkController.SendMessage(PhraseFramework.LimitReached,
                GoToMenuButtons, user);
            Status.UsersToEditorGiving.Remove(request);
            return;
        }

        if (!request.IsValid())
        {
            VkController.SendMessage(PhraseFramework.YourRequestNoLongerValid,
                GoToMenuButtons, user);
            Status.UsersToEditorGiving.Remove(request);
            return;
        }

        var groups = cachedGroups.Data.Where(cacheGroup => cacheGroup.Id == request.Group);
        if (!groups.Any())
        {
            Logger.LogCritical("Отсутвует запрашиваемая группа в кэше ({Group})", request.Group);
            VkController.SendMessage(PhraseFramework.Error("cache-error"),
                GoToMenuButtons, user);
            Status.UsersToEditorGiving.Remove(request);
            return;
        }

        await Task.Delay(TimeSpan.FromSeconds(2));

        var data = VkController.GetGroupInfo(request.Group);
        if (!data.Members.Contains(request.User))
        {
            VkController.SendMessage(PhraseFramework.YouHaveNotJoinedClub,
                IveJoinedButtons, user);
            await Task.Delay(500);
            ReplaceLongToGroup(ref cachedGroups, request);
            CacheFramework.SaveCache(cachedGroups);
            return;
        }

        string groupName;
        try
        {
            groupName = "@" + cachedGroups.Data.First(x => x.Id == request.Group).ShortName;
        }
        catch (InvalidOperationException)
        {
            groupName = $"@club{request.Group}";
        }

        Status.UsersToEditorGiving.Remove(request);

        try
        {
            VkController.EditManager(user, request.Group, ManagerRole.Editor);
        }
        catch (VkApiException e)
        {
            Logger.LogWarning("Не удалось выдать редактора {User}: {Exception}: {Message}",
                user, e.GetType(), e.Message);
            var logToShow = $"{e.GetType()}: {e.Message}";
            VkController.SendMessage(PhraseFramework.Error(logToShow),
                GoToMenuButtons, user);
            ReplaceLongToGroup(ref cachedGroups, request);
            if (priority) Status.AddLimitlessUserCoolDown(user);
            else Status.CoolDownUsers.Add(user);
            CacheFramework.SaveCache(cachedGroups);
            return;
        }

        await Task.Delay(1000);
        ReplaceLongToGroup(ref cachedGroups, request);
        if (priority) Status.AddLimitlessUserCoolDown(user);
        else Status.CoolDownUsers.Add(user);
        CacheFramework.SaveCache(cachedGroups);

        VkController.SendMessage(
            PhraseFramework.EditorSuccess(groupName),
            GoToMenuButtons, user);
    }

    private void ProcessIveJoinedLessThanFiftySubs(long user)
    {
        var cache = CacheFramework.LoadCache();
        var group = Enumerable.Reverse(cache.Data).ToList().First();
        var updatedGroup = VkController.GetGroupInfo(group.Id);
        if (updatedGroup.Members.Contains(user))
        {
            VkController.SendMessage(PhraseFramework.EditorAfterFiftySubs, GoToMenuButtons,
                user);
            return;
        }

        VkController.SendMessage(PhraseFramework.YouHaveNotJoinedClub, IveJoinedButtonsLessThanFiftySubs,
            user);
    }

    private void ProcessUnBanMe(long user)
    {
        var userIsRequested = CacheFramework.TryGetBannedRequestFromFullList(user, out var request);

        switch (userIsRequested)
        {
            case true when !request.IsAvailableToReview():
                var message = PhraseFramework.GetUnbanRequestOverview(request);
                VkController.SendMessage(message, GoToMenuButtons, user);
                return;

            case true when request.IsAvailableToReview():
                var overview = PhraseFramework
                    .YouCanSendAnotherUnbanRequest(PhraseFramework.GetUnbanRequestOverview(request));
                VkController.SendMessage(overview, UnbanRequestConfirm, user);
                return;

            default:
                VkController.SendMessage(
                    PhraseFramework.AreYouSureYouWantToSendUnbanRequest, UnbanRequestConfirm,
                    user);
                return;
        }
    }

    private void ProcessUnBanRequest(long user)
    {
        if (!CacheFramework.TryGetBannedPriority(user, out var priority))
        {
            SendErrorMessage(user);
            return;
        }

        if (priority is not BanPriority.Red and not BanPriority.Green and not BanPriority.White)
        {
            SendErrorMessage(user);
            return;
        }

        var isBanned = CacheFramework.TryGetBannedRequestFromFullList(user, out var request);

        switch (isBanned)
        {
            case true when request.IsAvailableToReview():
                CacheFramework.OverrideBannedRequest(request.UserId, new UnbanRequest(user, DateTime.Now));
                break;

            case true when !request.IsAvailableToReview():
                SendErrorMessage(user);
                return;

            default:
                CacheFramework.AddBannedRequest(new UnbanRequest(user, DateTime.Now));
                break;
        }

        VkController.SendMessage(PhraseFramework.UnbanRequestSent, GoToMenuButtons,
            user);
        VkController.SendMessage(PhraseFramework.NewUnbanRequest(user), null, Configuration.GetValue<long>("LogUser"));
    }
}
