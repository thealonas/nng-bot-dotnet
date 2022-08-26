using nng;
using nng_bot.API;
using nng_bot.Frameworks;
using nng.VkFrameworks;
using VkNet.Exception;

namespace nng_bot;

public class Startup
{
    private readonly EnvironmentConfiguration _configuration;

    public Startup()
    {
        _configuration = EnvironmentConfiguration.GetInstance();
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddMvc().AddNewtonsoftJson();
        try
        {
            services.AddSingleton(_ => new VkFramework(_configuration.Configuration.Auth.UserToken));
        }
        catch (Exception e)
        {
            if (e is UserAuthorizationFailException) Console.WriteLine("Возможно, неправильный токен пользователя");
            Console.WriteLine($"{e.GetType()}: {e.Message}");
            throw;
        }

        services.AddSingleton(_ => new VkFrameworkHttp(_configuration.Configuration.Auth.DialogGroupToken));
        services.AddSingleton<VkController>();
        services.AddSingleton<StatusFramework>();
        services.AddSingleton<OperationStatus>();
        services.AddSingleton<CacheFramework>();
        services.AddHostedService<CacheScheduledTask>();
        services.AddSingleton<PhraseFramework>();
        services.AddSingleton<CacheScheduledTaskProcessor>();

        services.AddSingleton<VkDialogHelper>();
        services.AddSingleton<VkDialogPayloadHandler>();
        services.AddSingleton<VkDialogAdminHandler>();
        services.AddSingleton<VkDialogUnbanRequests>();
        services.AddSingleton<DialogEntry>();
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
