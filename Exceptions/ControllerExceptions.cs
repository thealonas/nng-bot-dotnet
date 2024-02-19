using nng.DatabaseModels;

namespace nng_bot.Exceptions;

public class NoAvailableGroups : Exception
{
}

public class LessThanFiftySubs : Exception
{
    public readonly GroupInfo Group;

    public LessThanFiftySubs(GroupInfo group)
    {
        Group = group;
    }
}
