using nng_bot.Models;
using nng.DatabaseProviders;
using nng.Extensions;
using Redis.OM;

namespace nng_bot.Providers;

public class BotSettingsDatabaseProvider : DatabaseProvider<Configuration>
{
    public BotSettingsDatabaseProvider(ILogger<DatabaseProvider<Configuration>> logger,
        RedisConnectionProvider provider) : base(logger, provider)
    {
        if (!Collection.TryGetById("main", out var mainSettings))
            throw new InvalidOperationException();

        Settings = mainSettings;
    }

    public Configuration Settings { get; }
}
