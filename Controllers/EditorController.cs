using Microsoft.AspNetCore.Mvc;
using nng;
using Sentry;
using VkNet.Model;
using VkNet.Utils;

namespace nng_bot.Controllers;

[Route("")]
public class EditorController : Controller
{
    private readonly ConfigurationProvider _configuration;
    private readonly DialogEntry _entryPoint;

    private readonly long _group;
    private readonly string _secret;

    public EditorController(DialogEntry entryPoint, ConfigurationProvider provider)
    {
        _entryPoint = entryPoint;
        _configuration = provider;
        var config = provider.Configuration;
        _group = config.GroupId;
        _secret = config.GroupSecret;
    }

    private bool IsAllowed(long group, string secret)
    {
        return _group.Equals(group) && _secret.Equals(secret);
    }

    [HttpPost]
    public IActionResult Dialog([FromBody] VkEvent vkEvent)
    {
        if (!IsAllowed(vkEvent.GroupId, vkEvent.Secret)) return Ok("ok");

        switch (vkEvent.Type)
        {
            case "confirmation":
                return Ok(_configuration.Configuration.GroupConfirm);
            case "message_new":
            {
                var message = Message.FromJson(new VkResponse(vkEvent.Object));
                Task.Run(() => _entryPoint.Enter(message)).ContinueWith(task =>
                {
                    if (task is {IsFaulted: true, Exception: not null}) SentrySdk.CaptureException(task.Exception);
                });
                break;
            }
        }

        return Ok("ok");
    }
}
