using nng;
using nng_bot.Frameworks;

namespace nng_bot.Configs;

public static class UsersConfigurationProcessor
{
    private static readonly EnvironmentConfiguration EnvironmentConfiguration = EnvironmentConfiguration.GetInstance();

    public static UsersConfig UsersConfiguration => EnvironmentConfiguration.Configuration.Users;

    public static int EditorRestrictions => UsersConfiguration.EditorRestriction;

    public static int ManagersCeiling => UsersConfiguration.GroupManagersCeiling;

    public static bool IfUserPrioritized(long user)
    {
        return CacheFramework.LoadUsers().PriorityUsers.Any(x => x.Id == user) || IfUserIsAdmin(user);
    }

    public static bool IfUserIsAdmin(long user)
    {
        return UsersConfiguration.AdminUsers.Any(x => x == user);
    }
}
