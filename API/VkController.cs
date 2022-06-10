using nng.Exceptions;
using nng.VkFrameworks;
using nng_bot.Models;
using VkNet.Enums.SafetyEnums;

namespace nng_bot.API;

public class VkController
{
    public VkController(ILogger<VkController> logger,
        VkFrameworkHttp vkFrameworkHttp, VkFramework vkFramework)
    {
        Logger = logger;
        VkFrameworkHttp = vkFrameworkHttp;
        VkFramework = vkFramework;
    }

    private VkFrameworkHttp VkFrameworkHttp { get; }
    private VkFramework VkFramework { get; }
    private ILogger<VkController> Logger { get; }

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
        catch (VkFrameworkMethodException e)
        {
            Logger.LogWarning("{ExceptionType} {Message}", e.GetType(), e.Message);
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
}
