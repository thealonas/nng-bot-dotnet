using nng_bot.API;
using nng_bot.Frameworks;
using nng_bot.Providers;
using nng.DatabaseProviders;
using VkNet.Exception;

namespace nng_bot.BackgroundServices;

public class InfoPublisher : BackgroundService
{
    private readonly ILogger<InfoPublisher> _logger;
    private readonly BotSettingsDatabaseProvider _settings;
    private readonly StatusFramework _status;
    private readonly UsersDatabaseProvider _users;
    private readonly VkController _vkController;

    private Timer? _onlineTimer;
    private Timer? _statusTimer;

    public InfoPublisher(StatusFramework status, VkController vkController, BotSettingsDatabaseProvider settings,
        ILogger<InfoPublisher> logger, UsersDatabaseProvider users)
    {
        _status = status;
        _vkController = vkController;
        _settings = settings;
        _logger = logger;
        _users = users;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _statusTimer = new Timer(UpdateStatus, null, TimeSpan.Zero, TimeSpan.FromHours(3));
        _onlineTimer = new Timer(UpdateOnline, null, TimeSpan.FromSeconds(30), TimeSpan.FromHours(6));

        return Task.CompletedTask;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _onlineTimer?.Dispose();
        _statusTimer?.Dispose();

        return base.StopAsync(cancellationToken);
    }

    private void UpdateStatus(object? state)
    {
        // ReSharper disable once RemoveToList.2 | TODO: написать свой запрос в FT.SEARCH
        var count = _users.Collection.ToList().Count(x => x is {Banned: false, BannedInfo: null} ||
                                                          (!x.Banned && x is {BannedInfo: not null, Groups: not null}));
        _status.UpdateStatus(count);
    }

    private void UpdateOnline(object? state)
    {
        try
        {
            _vkController.TickOnline(_settings.Settings.GroupId);
        }
        catch (Exception e)
        {
            if (e is InvalidRequestException) return;

            _logger.LogWarning("Не получилось обновить онлайн: {Error}", $"{e.GetType()}: {e.Message}");
        }
    }
}
