using nng.Data;
using nng.Models;
using nng_bot.API;
using nng_bot.Models;
using VkNet.Exception;

namespace nng_bot.Frameworks;

public class CacheScheduledTaskProcessor
{
    public CacheScheduledTaskProcessor(ILogger<CacheScheduledTaskProcessor> logger, VkController api,
        IConfiguration configuration, CacheFramework cacheFramework,
        OperationStatus operationStatus, PhraseFramework phraseFramework)
    {
        Logger = logger;
        Api = api;
        Configuration = configuration;
        CacheFramework = cacheFramework;
        OperationStatus = operationStatus;
        PhraseFramework = phraseFramework;
    }

    public DateTime NextRun { get; set; }

    private OperationStatus OperationStatus { get; }
    private ILogger<CacheScheduledTaskProcessor> Logger { get; }
    private VkController Api { get; }
    private IConfiguration Configuration { get; }
    private CacheFramework CacheFramework { get; }
    private PhraseFramework PhraseFramework { get; }

    private bool InProgress
    {
        set => OperationStatus.BackingTaskInProgress = value;
    }

    public bool IsInstantUpdateAvailable => (NextRun - DateTime.Now).TotalMinutes > 2;

    public void ForceUpdateCache()
    {
        if (OperationStatus.BackingTaskInProgress || !IsInstantUpdateAvailable)
        {
            Logger.LogWarning("Кэш уже обновляется");
            return;
        }

        Logger.LogInformation("Запуск принудительного обновления кэша…");
        CacheFramework.DeleteCache(CacheFramework.CacheFilePath);
        UpdateCache(default);
    }

    public async void UpdateCache(object? state)
    {
        CacheFramework.CheckCache(CacheFramework.CacheFilePath);
        CacheFramework.CheckCache(CacheFramework.BannedUserFilePath);
        var data = await DataHelper.GetDataAsync(Configuration["DataUrl"]);

        var groups = data.GroupList.ToList();

        if (CacheFramework.IfCacheValid())
        {
            NextRun = DateTime.Now.AddHours(Configuration.GetValue<int>("Cache:UpdatePerHours"));
            Logger.LogInformation("Кэш валиден, обновлять не требуется");
            return;
        }

        Logger.LogInformation("Начинаем обновлять кэш…");
        InProgress = true;

        var saveData = new CacheData(new List<CacheGroup>(), new List<UserModel>(), DateTime.Now);

        OperationStatus.CoolDownUsers.Clear();
        OperationStatus.UnbannedUsers.Clear();
        OperationStatus.AdminRequests.Clear();
        OperationStatus.UsersToEditorGiving.Clear();
        OperationStatus.AdminUnbanRequestPages.Clear();
        OperationStatus.LimitlessLimit.Clear();

        foreach (var group in groups)
        {
            Logger.LogInformation("Обработка сообщества {Group}", group);
            try
            {
                saveData.Data.Add(Api.GetGroupInfo(group));
            }
            catch (VkApiException e)
            {
                Logger.LogError("Не удалось обработать сообщество {Group}: {Message}", group, e.Message);
            }

            await Task.Delay(1000);
        }

        Logger.LogInformation("Все группы были обновлены!");
        Logger.LogInformation("Обновляем список забаненных…");

        var users = data.Users.ToList();

        saveData.BannedUsers = users;
        Logger.LogInformation("Список забаненных обновлен!");

        saveData.CreatedOn = DateTime.Now;
        CacheFramework.SaveCache(saveData);
        Logger.LogInformation("Кэш обновлен успешно");
        InProgress = false;
        NextRun = DateTime.Now.AddHours(Configuration.GetValue<int>("Cache:UpdatePerHours"));
        foreach (var id in OperationStatus.UsersBotIsAvailable)
            Api.SendMessage(PhraseFramework.BotIsAvailableAgain, null, id);

        OperationStatus.UsersBotIsAvailable.Clear();
    }
}

public class CacheScheduledTask : BackgroundService
{
    public CacheScheduledTask(IConfiguration configuration, CacheScheduledTaskProcessor cacheScheduledTaskProcessor)
    {
        Configuration = configuration;
        CacheScheduledTaskProcessor = cacheScheduledTaskProcessor;
        Timer = null!;
    }

    // ReSharper disable once UnusedAutoPropertyAccessor.Local
    private Timer Timer { get; set; }

    private IConfiguration Configuration { get; }
    private CacheScheduledTaskProcessor CacheScheduledTaskProcessor { get; }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var hours = long.Parse(Configuration["Cache:UpdatePerHours"]);
        var updateAtStart = Configuration.GetValue<bool>("Cache:UpdateAtStart");
        CacheScheduledTaskProcessor.NextRun = updateAtStart ? DateTime.Now : DateTime.Now.AddHours(hours);
        Timer = new Timer(CacheScheduledTaskProcessor.UpdateCache, stoppingToken,
            updateAtStart ? TimeSpan.Zero : TimeSpan.FromHours(hours),
            TimeSpan.FromHours(hours));
        return Task.CompletedTask;
    }
}
