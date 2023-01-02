using nng_bot.Frameworks;

namespace nng;

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
