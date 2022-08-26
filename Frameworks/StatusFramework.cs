using nng_bot.API;
using nng.VkFrameworks;
using VkNet.Exception;
using VkNet.Model.RequestParams;

namespace nng_bot.Frameworks;

public class StatusFramework
{
    private readonly VkController _controller;
    private readonly long _groupId;
    private readonly ILogger<StatusFramework> _status;

    public StatusFramework(VkController controller, ILogger<StatusFramework> status)
    {
        _controller = controller;
        _status = status;
        var config = EnvironmentConfiguration.GetInstance().Configuration;
        _groupId = config.Auth.DialogGroupId;
    }

    public long GetConversationsCount()
    {
        try
        {
            var group = (ulong?) _groupId;

            var results = VkFrameworkExecution.ExecuteWithReturn(() =>
                _controller.GroupFramework.Messages.GetConversations(new GetConversationsParams
                {
                    Count = 0,
                    Extended = false,
                    GroupId = group,
                    Offset = 0
                }));

            return results.Count;
        }
        catch (VkApiException e)
        {
            _status.LogWarning("{Type}: {Message}", e.GetType(), e.Message);
            return 0;
        }
    }

    public void UpdateStatus(long count)
    {
        _controller.SetEditorStatus(count);
    }
}
