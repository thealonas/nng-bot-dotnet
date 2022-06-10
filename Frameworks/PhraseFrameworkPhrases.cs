using nng_bot.Models;

namespace nng_bot.Frameworks;

public partial class PhraseFramework
{
    public string AdminPanel => GetPhrase("AdminPanel");
    public string AgreeWithRules => GetPhrase("AgreeWithRules");
    public string AreYouSureYouWantToSendUnbanRequest => GetPhrase("AreYouSureYouWantToSendUnbanRequest");
    public string BotIsAvailableAgain => GetPhrase("BotIsAvailableAgain");
    public string CacheCleaningStarted => GetPhrase("CacheCleaningStarted");
    public string CacheIsAboutToUpdateItself => GetPhrase("CacheIsAboutToUpdateItself");

    public string CommandNotFound => GetPhrase("CommandNotFound");
    public string EditorAfterFiftySubs => GetPhrase("EditorAfterFiftySubs");
    public string LimitPanel => GetPhrase("LimitPanel");
    public string LimitReached => GetPhrase("LimitReached");
    public string MainMenu => GetPhrase("MainMenu");
    public string NoAvailableSlots => GetPhrase("NoAvailableSlots");
    public string NoBannedRequests => GetPhrase("NoBannedRequests");
    public string TempUnavailable => GetPhrase("TempUnavailable");
    public string UnbanRequestSent => GetPhrase("UnbanRequestSent");
    public string UserIdIsNotInteger => GetPhrase("UserIdIsNotInteger");
    public string WriteDownUserIdToShowHisProfile => GetPhrase("WriteDownUserIdToShowHisProfile");
    public string YouAreOnCoolDown => GetPhrase("YouAreOnCoolDown");
    public string YouHaveBeenUnbanned => GetPhrase("YouHaveBeenUnbanned");
    private string YouHaveNoEditor => GetPhrase("YouHaveNoEditor");
    public string YouHaveNotJoinedClub => GetPhrase("YouHaveNotJoinedClub");
    public string YourAccountIsTooYoung => GetPhrase("YourAccountIsTooYoung");
    public string YourRequestHasBeenRejected => GetPhrase("YourRequestHasBeenRejected");
    public string YourRequestNoLongerValid => GetPhrase("YourRequestNoLongerValid");

    public string CacheOutput(int groups, string slots, string members, long membersWithoutDuplicates, long managers,
        long managersWithoutDuplicates)
    {
        return GetPhrase("CacheOutput").SetGroups(groups).SetSlots(slots)
            .SetMembers(members)
            .SetMembersWithoutDuplicates(membersWithoutDuplicates)
            .SetManagers(managers)
            .SetManagersWithoutDuplicates(managersWithoutDuplicates);
    }

    public string DeletedUnbanRequests(string data)
    {
        return GetPhrase("DeletedUnbanRequests").SetCount(data);
    }

    public string EditorRestrictionChanged(long newLimit)
    {
        return GetPhrase("EditorRestrictionChanged").SetNew(newLimit);
    }

    public string EditorSuccess(string id)
    {
        return GetPhrase("EditorSuccess").SetId(id);
    }

    public string Error(string log)
    {
        return GetPhrase("Error").SetLog(log);
    }

    public string LessThanFiftySubs(CacheGroup group)
    {
        return GetPhrase("LessThanFiftySubs").SetId(group);
    }

    public string NewUnbanRequest(long id)
    {
        return GetPhrase("NewUnbanRequest").SetId(id);
    }

    public string PleaseJoinGroup(CacheGroup group)
    {
        return GetPhrase("PleaseJoinGroup").SetId(group);
    }

    private string Profile(string name, long id, string date, string dateNewLine, string ban, string editor,
        string editorCount)
    {
        return GetPhrase("Profile").SetName(name).SetId(id).SetDate(date).SetDateNewLine(dateNewLine).SetBan(ban)
            .SetEditorCounter(editorCount).SetEditor(editor);
    }

    public string UnbanRequestAdminOverview(string info, string status, string time, string profile)
    {
        return GetPhrase("UnbanRequestAdminOverview").SetInfo(info).SetStatus(status).SetTime(time).SetProfile(profile);
    }

    private string UnbanRequestOverview(string time, string status)
    {
        return GetPhrase("UnbanRequestOverview").SetTime(time).SetStatus(status);
    }

    public string UserAlreadyAdmin(long id)
    {
        return GetPhrase("UserAlreadyAdmin").SetId(id);
    }

    public string UserAlreadyBanned(long id)
    {
        return GetPhrase("UserAlreadyBanned").SetId(id);
    }

    public string UserAlreadyLimitless(long id)
    {
        return GetPhrase("UserAlreadyLimitless").SetId(id);
    }

    public string UserBannedSuccessfully(long id)
    {
        return GetPhrase("UserBannedSuccessfully").SetId(id);
    }

    public string UserHasBeenUnbanned(long id)
    {
        return GetPhrase("UserHasBeenUnbanned").SetId(id);
    }

    public string UserIsNotAdmin(long id)
    {
        return GetPhrase("UserIsNotAdmin").SetId(id);
    }

    public string UserIsNotBanned(long id)
    {
        return GetPhrase("UserIsNotBanned").SetId(id);
    }

    public string UserIsNotLimitless(long id)
    {
        return GetPhrase("UserIsNotLimitless").SetId(id);
    }

    public string UserMadeAdminSuccessfully(long id)
    {
        return GetPhrase("UserMadeAdminSuccessfully").SetId(id);
    }

    public string UserMadeLimitlessSuccessfully(long id)
    {
        return GetPhrase("UserMadeLimitlessSuccessfully").SetId(id);
    }

    public string UserRemovedFromAdminSuccessfully(long id)
    {
        return GetPhrase("UserRemovedFromAdminSuccessfully").SetId(id);
    }

    public string UserRemovedFromLimitlessSuccessfully(long id)
    {
        return GetPhrase("UserRemovedFromLimitlessSuccessfully").SetId(id);
    }

    public string UserRequestHasBeenRejected(long id)
    {
        return GetPhrase("UserRequestHasBeenRejected").SetId(id);
    }

    public string UserUnbannedSuccessfully(long id)
    {
        return GetPhrase("UserUnbannedSuccessfully").SetId(id);
    }

    public string WriteDownAdminUserId(string current)
    {
        return GetPhrase("WriteDownAdminUserId").SetCurrent(current);
    }

    public string WriteDownBanUserId(string current)
    {
        return GetPhrase("WriteDownBanUserId").SetCurrent(current);
    }

    public string WriteDownLimitlessUserId(string current)
    {
        return GetPhrase("WriteDownLimitlessUserId").SetCurrent(current);
    }

    public string WriteDownNewRestriction(long current)
    {
        return GetPhrase("WriteDownNewRestriction").SetCurrent(current);
    }

    public string YouCanSendAnotherUnbanRequest(string oldRequest)
    {
        return GetPhrase("YouCanSendAnotherUnbanRequest").SetOldRequest(oldRequest);
    }
}
