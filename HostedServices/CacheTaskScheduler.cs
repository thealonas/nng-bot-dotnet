using nng_bot.API;
using nng_bot.Frameworks;
using nng_bot.Models;
using nng.Helpers;
using nng.Models;
using VkNet.Exception;

namespace nng;

public class CacheScheduledTaskProcessor
{
    private readonly StatusFramework _framework;

    public CacheScheduledTaskProcessor(ILogger<CacheScheduledTaskProcessor> logger, VkController api,
        CacheFramework cacheFramework, OperationStatus operationStatus, PhraseFramework phraseFramework,
        StatusFramework framework)
    {
        _framework = framework;
        Logger = logger;
        Api = api;
        CacheFramework = cacheFramework;
        OperationStatus = operationStatus;
        PhraseFramework = phraseFramework;
        Config = EnvironmentConfiguration.GetInstance().Configuration;
    }

    public DateTime NextRun { get; set; }

    private OperationStatus OperationStatus { get; }
    private ILogger<CacheScheduledTaskProcessor> Logger { get; }
    private VkController Api { get; }
    private CacheFramework CacheFramework { get; }
    private PhraseFramework PhraseFramework { get; }
    private Config Config { get; }

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

    public void UpdateCache(object? state)
    {
        Logger.LogDebug("[58] OperationStatus.UsersBotIsAvailable.Count: {Count}",
            OperationStatus.UsersBotIsAvailable.Count);

        CacheFramework.CheckCache(CacheFramework.CacheFilePath);
        CacheFramework.CheckCache(CacheFramework.BannedUserFilePath);
        var data = DataHelper.GetDataAsync(Config.DataUrl).GetAwaiter().GetResult();

        var groups = data.GroupList.ToList();

        if (CacheFramework.IfCacheValid())
        {
            NextRun = DateTime.Now.AddHours(Config.Cache.UpdatePerHours);
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
        OperationStatus.UsersAskedForEditor.Clear();
        OperationStatus.LimitlessLimit.Clear();
        OperationStatus.UsersBotIsAvailable.Clear();

        Logger.LogDebug("[87] OperationStatus.UsersBotIsAvailable.Count: {Count}",
            OperationStatus.UsersBotIsAvailable.Count);

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

            Task.Delay(1000).GetAwaiter().GetResult();
        }

        Logger.LogInformation("Все группы были обновлены!");
        Logger.LogInformation("Обновляем список забаненных…");

        var users = data.Users.ToList();

        saveData.BannedUsers = users;
        Logger.LogInformation("Список забаненных обновлен!");

        saveData.CreatedOn = DateTime.Now;
        CacheFramework.SaveCache(saveData);

        Logger.LogInformation("Обновляем диалоги для статуса…");
        var count = _framework.GetConversationsCount();
        Logger.LogInformation("Количество диалогов для статуса: {Count}", count);
        _framework.UpdateStatus(count);
        Logger.LogInformation("Статус обновлен");

        Logger.LogInformation("Кэш обновлен успешно");
        InProgress = false;
        NextRun = DateTime.Now.AddHours(Config.Cache.UpdatePerHours);
        foreach (var id in OperationStatus.UsersBotIsAvailable)
            Api.SendMessage(PhraseFramework.BotIsAvailableAgain, null, id);

        OperationStatus.UsersBotIsAvailable.Clear();
    }
}

public class CacheScheduledTask : BackgroundService
{
    public CacheScheduledTask(CacheScheduledTaskProcessor cacheScheduledTaskProcessor)
    {
        CacheScheduledTaskProcessor = cacheScheduledTaskProcessor;
        Config = EnvironmentConfiguration.GetInstance().Configuration;
        Timer = null!;
    }

    // ReSharper disable once UnusedAutoPropertyAccessor.Local
    private Timer Timer { get; set; }

    private CacheScheduledTaskProcessor CacheScheduledTaskProcessor { get; }
    private Config Config { get; }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var hours = Config.Cache.UpdatePerHours;
        var updateAtStart = Config.Cache.UpdateAtStart;
        CacheScheduledTaskProcessor.NextRun = updateAtStart ? DateTime.Now : DateTime.Now.AddHours(hours);
        Timer = new Timer(CacheScheduledTaskProcessor.UpdateCache, stoppingToken,
            updateAtStart ? TimeSpan.Zero : TimeSpan.FromHours(hours),
            TimeSpan.FromHours(hours));
        return Task.CompletedTask;
    }
}
