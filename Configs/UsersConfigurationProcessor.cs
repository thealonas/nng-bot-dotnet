using Newtonsoft.Json;
using nng_bot.Models;

namespace nng_bot.Configs;

public static class UsersConfigurationProcessor
{
    public static UsersConfig UsersConfiguration
    {
        get
        {
            var config = File.ReadAllText("Configs/users.json");
            return JsonConvert.DeserializeObject<UsersConfig>(config);
        }
    }

    public static int EditorRestrictions => UsersConfiguration.EditorRestriction;

    public static int ManagersCeiling => UsersConfiguration.GroupManagersCeiling;

    public static void SaveUsersConfig(UsersConfig config)
    {
        var json = JsonConvert.SerializeObject(config, Formatting.Indented);
        File.WriteAllText("Configs/users.json", json);
    }

    public static bool IfUserPrioritized(long user)
    {
        return UsersConfiguration.PriorityUsers.Any(x => x == user);
    }

    public static bool IfUserIsBannedInBot(long user)
    {
        return UsersConfiguration.BannedUsers.Any(x => x == user);
    }

    public static bool IfUserIsAdmin(long user)
    {
        return UsersConfiguration.AdminUsers.Any(x => x == user);
    }
}
