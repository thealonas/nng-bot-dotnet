using nng;

namespace nng_bot.Extensions;

public static class CacheGroupExtensions
{
    public static bool IsManager(this CacheGroup group, long target)
    {
        return group.Managers.Contains(target);
    }

    public static bool IsMember(this CacheGroup group, long target)
    {
        return group.Members.Contains(target);
    }
}
