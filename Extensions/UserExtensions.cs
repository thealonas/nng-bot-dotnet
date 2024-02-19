using nng.DatabaseModels;

namespace nng_bot.Extensions;

public static class UserExtensions
{
    public static bool HasPriority(this User? user)
    {
        if (user is null) return false;
        return user.Thanks || user.Admin;
    }
}
