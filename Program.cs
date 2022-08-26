using System.Text;
using nng.Constants;
using nng.Helpers;
using Sentry;
using Sentry.Extensibility;

namespace nng_bot;

public static class Program
{
    public static void Main(string[] args)
    {
        Console.OutputEncoding = OperatingSystem.IsWindows() ? Encoding.Unicode : Encoding.UTF8;
        CreateHostBuilder(args).Build().Run();
    }

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        var configs = new[]
        {
            "phrases.json"
        };
        return Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.SetBasePath(Directory.GetCurrentDirectory());

                foreach (var path in configs) configuration.AddJsonFile($"Configs/{path}", false, true);

                configuration.AddEnvironmentVariables();
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Information);
            })
            .ConfigureWebHostDefaults(builder =>
            {
                builder.UseKestrel();

                builder.UseUrls("http://*:1220");

                builder.UseSentry(o =>
                {
                    o.Dsn = "https://7e8320f873f54947bbe348579de35a65@o555933.ingest.sentry.io/6151189";
                    o.MaxRequestBodySize = RequestSize.Always;
                    o.SendDefaultPii = true;
                    o.MinimumBreadcrumbLevel = LogLevel.Debug;
                    o.MinimumEventLevel = LogLevel.Warning;
                    o.AttachStacktrace = true;
                    o.Debug = false;
                    o.DiagnosticLevel = SentryLevel.Error;

                    var targetEnv = EnvironmentHelper.GetString(EnvironmentConstants.Sentry, "prod");
                    o.Environment = targetEnv;
                });

                builder.UseStartup<Startup>();
            });
    }
}
