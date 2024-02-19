using nng_bot.Providers;
using nng.VkFrameworks;
using VkNet;
using VkNet.Enums.SafetyEnums;
using VkNet.Exception;
using VkNet.Model;

namespace nng_bot.API;

public class VkController
{
    private readonly ConfigurationProvider _configurationProvider;
    private readonly Random _random;
    private readonly VkFrameworkProvider _vkFrameworkProvider;

    public VkController(ILogger<VkController> logger, VkFrameworkProvider vkFrameworkProvider,
        ConfigurationProvider configurationProvider)
    {
        _vkFrameworkProvider = vkFrameworkProvider;
        _configurationProvider = configurationProvider;
        Logger = logger;

        var configuration = configurationProvider.Configuration;

        VkFrameworkHttp = new VkFrameworkHttp(configuration.GroupToken);

        GroupFramework = new VkApi();
        GroupFramework.Authorize(new ApiAuthParams
        {
            AccessToken = configuration.GroupToken
        });

        HttpClient = new HttpClient();
        _random = new Random();
    }

    private VkFrameworkHttp VkFrameworkHttp { get; }
    public VkFramework VkFramework => _vkFrameworkProvider.VkFramework;
    public VkApi GroupFramework { get; }
    private ILogger<VkController> Logger { get; }
    private HttpClient HttpClient { get; }

    public void EditManager(long user, long group, ManagerRole role)
    {
        VkFramework.EditManager(user, group, role);
    }

    public void SendMessage(string? message, string? keyboard, long peer, bool doNotParseLinks = true)
    {
        try
        {
            VkFrameworkHttp.SendMessage(message, keyboard, peer, doNotParseLinks);
        }
        catch (VkApiException e)
        {
            Logger.LogWarning("{ExceptionType}: {Message}", e.GetType(), e.Message);
        }
    }

    public void SendSticker(long peer, long stickerId)
    {
        try
        {
            HttpClient.PostAsync("https://api.vk.com/method/messages.send", new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    {"random_id", _random.Next(int.MaxValue).ToString()},
                    {"peer_id", peer.ToString()},
                    {"sticker_id", stickerId.ToString()},
                    {"access_token", _configurationProvider.Configuration.GroupToken},
                    {"v", "5.131"}
                }));
        }
        catch (VkApiException e)
        {
            Logger.LogWarning("{ExceptionType}: {Message}", e.GetType(), e.Message);
        }
    }

    public void SetEditorStatus(long group, string text)
    {
        try
        {
            VkFramework.SetGroupStatus(group, text);
        }
        catch (VkApiException e)
        {
            Logger.LogWarning("{ExceptionType}: {Message}", e.GetType(), e.Message);
        }
    }

    public void TickOnline(long group)
    {
        VkFrameworkExecution.Execute(() => { VkFramework.Api.Groups.EnableOnline((ulong) group); });
    }
}
