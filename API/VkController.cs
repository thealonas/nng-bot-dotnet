using nng.VkFrameworks;
using nng_bot.Models;
using VkNet;
using VkNet.Enums.SafetyEnums;
using VkNet.Exception;
using VkNet.Model;

namespace nng_bot.API;

public class VkController
{
    public VkController(ILogger<VkController> logger, VkFramework vkFramework,
        VkFrameworkHttp vkFrameworkHttp, IConfiguration configuration)
    {
        Logger = logger;
        VkFrameworkHttp = vkFrameworkHttp;
        VkFramework = vkFramework;
        Configuration = configuration;

        GroupFramework = new VkApi();
        GroupFramework.Authorize(new ApiAuthParams
        {
            AccessToken = configuration["Auth:DialogGroupToken"]
        });
    }

    private VkFrameworkHttp VkFrameworkHttp { get; }
    private VkFramework VkFramework { get; }
    public VkApi GroupFramework { get; }
    private ILogger<VkController> Logger { get; }
    private IConfiguration Configuration { get; }

    public void EditManager(long user, long group, ManagerRole role)
    {
        VkFramework.EditManager(user, group, role);
    }

    public void SendMessage(string? message, string? keyboard, long peer)
    {
        try
        {
            VkFrameworkHttp.SendMessage(message, keyboard, peer);
        }
        catch (VkApiException e)
        {
            Logger.LogWarning("{ExceptionType}: {Message}", e.GetType(), e.Message);
        }
    }

    public CacheGroup GetGroupInfo(long group)
    {
        var returnCache = new CacheGroup
        {
            Id = group
        };

        var data = VkFramework.GetGroupDataLegacy(group);
        returnCache.Members = data.AllUsers.Select(x => (long) x).ToList();
        returnCache.Managers = data.Managers.Select(x => x.Id).ToList();
        returnCache.MembersTotalCount = data.Count;
        returnCache.ShortName = data.ShortName;
        returnCache.ManagerTotalCount = data.ManagerCount;
        return returnCache;
    }

    public void SetEditorStatus(long count)
    {
        try
        {
            VkFramework.SetGroupStatus(Configuration.GetValue<long>("Auth:DialogGroupId"),
                $"🤠 всего пользователей: {count}");
        }
        catch (VkApiException e)
        {
            Logger.LogWarning("{ExceptionType}: {Message}", e.GetType(), e.Message);
        }
    }
}
