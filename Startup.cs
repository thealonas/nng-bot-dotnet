using nng;
using nng.VkFrameworks;
using nng_bot.API;
using nng_bot.Frameworks;
using VkNet.Exception;

namespace nng_bot;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    private IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddMvc().AddNewtonsoftJson();
        try
        {
            services.AddSingleton(_ => new VkFramework(Configuration.GetSection("Auth:UserToken").Value));
        }
        catch (Exception e)
        {
            if (e is UserAuthorizationFailException) Console.WriteLine("Возможно, неправильный токен пользователя");
            Console.WriteLine($"{e.GetType()}: {e.Message}");
            throw;
        }

        services.AddSingleton(_ => new VkFrameworkHttp(Configuration.GetSection("Auth:DialogGroupToken").Value));
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
