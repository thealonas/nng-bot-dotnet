using nng;
using nng_bot.Models;
using nng.Constants;
using nng.Helpers;

namespace nng_bot.Frameworks;

public class EnvironmentConfiguration
{
    private static EnvironmentConfiguration? _instance;

    private EnvironmentConfiguration()
    {
        Configuration = GetConfigFromEnv();
    }

    public Config Configuration { get; }

    public static EnvironmentConfiguration GetInstance()
    {
        return _instance ??= new EnvironmentConfiguration();
    }

    private static Config GetConfigFromEnv()
    {
        var dataUrl = EnvironmentHelper.GetString(EnvironmentConstants.DataUrl);
        var editorGrant = EnvironmentHelper.GetBoolean(nameof(Config.EditorGrantEnabled), true);
        var logUser = EnvironmentHelper.GetLong(EnvironmentConstants.LogUser);

        var userToken = EnvironmentHelper.GetString(EnvironmentConstants.UserToken);
        var dialogGroupId = EnvironmentHelper.GetLong(EnvironmentConstants.DialogGroupId);
        var dialogGroupToken = EnvironmentHelper.GetString(EnvironmentConstants.DialogGroupToken);
        var dialogGroupSecret = EnvironmentHelper.GetString(EnvironmentConstants.DialogGroupSecret);
        var dialogGroupConfirm = EnvironmentHelper.GetString(EnvironmentConstants.DialogGroupConfirm);

        var cacheUpdateAtStart = EnvironmentHelper.GetBoolean(nameof(Config.Cache.UpdateAtStart), true);
        var cacheUpdatePerHours = EnvironmentHelper.GetInt(nameof(Config.Cache.UpdatePerHours), 4);

        var configAuth = new AuthConfig(userToken, dialogGroupId, dialogGroupToken, dialogGroupSecret,
            dialogGroupConfirm);
        var configCache = new CacheConfig(cacheUpdateAtStart, cacheUpdatePerHours);

        var users = new UsersConfig
        {
            AdminUsers = EnvironmentHelper.GetString("AdminUsers").Split(",").Select(long.Parse).ToList(),
            EditorRestriction = EnvironmentHelper.GetInt("EditorRestriction", 20),
            GroupManagersCeiling = EnvironmentHelper.GetInt("GroupManagersCeiling", 100)
        };

        return new Config(dataUrl, editorGrant, logUser, configAuth, configCache, users);
    }
}
