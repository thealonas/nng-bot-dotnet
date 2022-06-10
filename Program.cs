using System.Text;

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
        var config = new ConfigurationBuilder()
            .AddCommandLine(args)
            .AddJsonFile("Configs/appsettings.json")
            .Build();
        var configs = new[]
        {
            "appsettings.json",
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
            })
            .ConfigureWebHostDefaults(builder =>
            {
                builder.UseKestrel();

                builder.UseSentry(o => { o.Environment = config["Sentry:Environment"]; });

                builder.UseStartup<Startup>();
            });
    }
}
