using nng;

namespace nng_bot.Exceptions;

public class NoAvailableGroups : Exception
{
}

public class LessThanFiftySubs : Exception
{
    public readonly CacheGroup Group;

    public LessThanFiftySubs(CacheGroup group)
    {
        Group = group;
    }
}
