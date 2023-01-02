using nng;
using nng_bot.Frameworks;
using nng_bot.Models;
using nng.VkFrameworks;
using VkNet;
using VkNet.Enums.SafetyEnums;
using VkNet.Exception;
using VkNet.Model;

namespace nng_bot.API;

public class VkController
{
    public VkController(ILogger<VkController> logger, VkFramework vkFramework,
        VkFrameworkHttp vkFrameworkHttp)
    {
        Logger = logger;
        VkFrameworkHttp = vkFrameworkHttp;
        VkFramework = vkFramework;

        GroupFramework = new VkApi();
        Configuration = EnvironmentConfiguration.GetInstance().Configuration;
        GroupFramework.Authorize(new ApiAuthParams
        {
            AccessToken = Configuration.Auth.DialogGroupToken
        });
    }

    private VkFrameworkHttp VkFrameworkHttp { get; }
    private VkFramework VkFramework { get; }
    public VkApi GroupFramework { get; }
    private ILogger<VkController> Logger { get; }
    private Config Configuration { get; }

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
            VkFramework.SetGroupStatus(Configuration.Auth.DialogGroupId,
                $"🤠 всего пользователей: {count}");
        }
        catch (VkApiException e)
        {
            Logger.LogWarning("{ExceptionType}: {Message}", e.GetType(), e.Message);
        }
    }
}
