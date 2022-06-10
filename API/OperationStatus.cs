using nng_bot.Extensions;
using nng_bot.Models;

namespace nng_bot.API;

public class OperationStatus
{
    public OperationStatus()
    {
        AdminUnbanRequestPages = new Dictionary<long, int>();
        CoolDownUsers = new HashSet<long>();
        UsersToEditorGiving = new HashSet<EditorRequest>();
        AdminRequests = new HashSet<AdminRequest>();
        BackingTaskInProgress = false;
        UnbannedUsers = new HashSet<long>();
        LimitlessLimit = new Dictionary<long, DateTime>();
        UsersBotIsAvailable = new HashSet<long>();
    }

    public HashSet<EditorRequest> UsersToEditorGiving { get; }
    public HashSet<AdminRequest> AdminRequests { get; }

    public bool BackingTaskInProgress { get; set; }

    public HashSet<long> CoolDownUsers { get; }
    public Dictionary<long, DateTime> LimitlessLimit { get; }
    public HashSet<long> UnbannedUsers { get; }
    public HashSet<long> UsersBotIsAvailable { get; }
    public Dictionary<long, int> AdminUnbanRequestPages { get; }

    public void AddLimitlessUserCoolDown(long user)
    {
        LimitlessLimit.RemoveIfExists(user);
        LimitlessLimit.Add(user, DateTime.Now);
    }

    public void AddEditorRequest(EditorRequest request)
    {
        UsersToEditorGiving.RemoveWhere(x => x.User == request.User);
        UsersToEditorGiving.Add(request);
    }

    public void AddAdminRequest(AdminRequest request)
    {
        AdminRequests.RemoveWhere(x => x.Admin == request.Admin || !x.IsValid);
        AdminRequests.Add(request);
    }

    public bool IfUserIsUnbanned(long user)
    {
        return UnbannedUsers.Contains(user);
    }

    public int GetAdminCurrentUnbanRequestPageIndex(long user)
    {
        return !AdminUnbanRequestPages.ContainsKey(user) ? 0 : AdminUnbanRequestPages[user];
    }

    public void SetAdminCurrentUnbanRequestPageIndex(long user, int value)
    {
        AdminUnbanRequestPages.RemoveIfExists(user);
        AdminUnbanRequestPages.Add(user, value);
    }
}
