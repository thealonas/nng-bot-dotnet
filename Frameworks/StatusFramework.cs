using nng_bot.API;

namespace nng_bot.Frameworks;

public class StatusFramework
{
    private readonly VkController _controller;
    private readonly long _groupId;

    public StatusFramework(VkController controller, ConfigurationProvider config)
    {
        _controller = controller;
        _groupId = config.Configuration.GroupId;
    }

    public void UpdateStatus(long count)
    {
        _controller.SetEditorStatus(_groupId, $"🤠 всего пользователей: {count}");
    }
}
