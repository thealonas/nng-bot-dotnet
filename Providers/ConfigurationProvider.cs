using nng_bot.Models;
using nng.Extensions;

namespace nng_bot.Providers;

public class ConfigurationProvider
{
    private readonly BotSettingsDatabaseProvider _settings;

    public ConfigurationProvider(BotSettingsDatabaseProvider settings)
    {
        _settings = settings;
    }

    public Configuration Configuration
    {
        get
        {
            if (!_settings.Collection.TryGetById("main", out var config))
                throw new Exception("Bot configuration not found");

            return config;
        }
    }
}
