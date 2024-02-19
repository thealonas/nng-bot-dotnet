using nng.Exceptions;
using nng.Misc;

namespace nng_bot.Providers;

public static class UsersRegistrationDateProvider
{
    private static readonly Dictionary<long, DateTime> UsersRegistrationDate = new();

    public static DateTime? GetRegistrationDate(long user)
    {
        try
        {
            if (UsersRegistrationDate.ContainsKey(user)) return UsersRegistrationDate[user];
            UsersRegistrationDate[user] = VkAccountAgeChecker.GetAccountAge(user);
            return UsersRegistrationDate[user];
        }
        catch (VkFrameworkMethodException)
        {
            return null;
        }
    }
}
