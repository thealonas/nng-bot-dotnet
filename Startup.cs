using nng;
using nng_bot.API;
using nng_bot.BackgroundServices;
using nng_bot.Frameworks;
using nng_bot.Providers;
using nng.DatabaseProviders;
using nng.Helpers;
using Redis.OM;

namespace nng_bot;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddMvc().AddNewtonsoftJson();

        var redisConnectionProvider = new RedisConnectionProvider(EnvironmentHelper.GetString("REDIS_URL"));

        services.AddSingleton(redisConnectionProvider);

        services.AddSingleton<BotSettingsDatabaseProvider>();
        services.AddSingleton<ConfigurationProvider>();
        services.AddSingleton<TokensDatabaseProvider>();
        services.AddSingleton<SettingsDatabaseProvider>();
        services.AddSingleton<GroupsDatabaseProvider>();
        services.AddSingleton<GroupStatsDatabaseProvider>();
        services.AddSingleton<UsersDatabaseProvider>();

        services.AddSingleton<UserFramework>();

        services.AddSingleton<VkFrameworkProvider>();

        services.AddSingleton<VkController>();
        services.AddSingleton<StatusFramework>();
        services.AddSingleton<OperationStatus>();
        services.AddSingleton<PhraseFramework>();

        services.AddSingleton<VkDialogHelper>();
        services.AddSingleton<VkDialogPayloadHandler>();
        services.AddSingleton<DialogEntry>();

        services.AddSingleton<CooldownFramework>();

        services.AddHostedService<InfoPublisher>();
    }

    private static void LoadInstant(IApplicationBuilder app)
    {
        app.ApplicationServices.GetService<OperationStatus>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        LoadInstant(app);

        app.UseRouting();

        app.UseAuthorization();

        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }
}
