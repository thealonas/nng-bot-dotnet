using nng_bot.Frameworks;

namespace nng_bot.Models;

public readonly struct CacheObject
{
    public int TotalManagers => Managers.Count;
    public readonly int TotalMembers;

    public readonly List<long> Members;
    public readonly List<long> Managers;

    public CacheObject(long group)
    {
        var cache = CacheFramework.LoadCache();
        var groupData = cache.Data.First(x => x.Id == group);
        Members = groupData.Members;
        Managers = groupData.Managers;
        TotalMembers = groupData.MembersTotalCount;
    }
}

public class CacheObjectList
{
    public CacheObjectList(IEnumerable<CacheObject> cacheObjects)
    {
        CacheObjects = cacheObjects.ToList();

        var users = new List<long>();
        var managers = new List<long>();
        foreach (var cacheObject in CacheObjects)
        {
            users.AddRange(cacheObject.Members);
            managers.AddRange(cacheObject.Managers);
        }

        users = users.Distinct().ToList();
        managers = managers.Distinct().ToList();

        TotalMembersWithoutDuplicates = users.Count;
        TotalManagersWithoutDuplicates = managers.Count;

        TotalSlots = CacheObjects.Count * 100;
        TotalBusySlots = CacheObjects.Select(x => x.TotalManagers).Sum();
    }

    private List<CacheObject> CacheObjects { get; }
    public int TotalBusySlots { get; }
    public int TotalManagersWithoutDuplicates { get; }
    public int TotalMembersWithoutDuplicates { get; }

    public int TotalSlots { get; }

    public int TotalMembers => CacheObjects.Sum(x => x.TotalMembers);

    public int TotalManagers => CacheObjects.Sum(x => x.TotalManagers);
}
