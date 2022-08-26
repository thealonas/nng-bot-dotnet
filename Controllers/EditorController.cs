using Microsoft.AspNetCore.Mvc;
using nng;
using nng_bot.Frameworks;
using nng_bot.Models;
using VkNet.Model;
using VkNet.Utils;

namespace nng_bot.Controllers;

[ApiController]
[Route("")]
public class EditorController : Controller
{
    private readonly Config _configuration;
    private readonly DialogEntry _entryPoint;

    public EditorController(DialogEntry entryPoint)
    {
        _configuration = EnvironmentConfiguration.GetInstance().Configuration;
        _entryPoint = entryPoint;
    }

    private bool IsAllowed(long group, string secret)
    {
        return _configuration.Auth.DialogGroupId.Equals(group) &&
               _configuration.Auth.DialogGroupSecret.Equals(secret);
    }

    [HttpPost]
    public IActionResult Dialog([FromBody] VkEvent vkEvent)
    {
        if (!IsAllowed(vkEvent.GroupId, vkEvent.Secret)) return Ok("ok");

        switch (vkEvent.Type)
        {
            case "confirmation":
                return Ok(_configuration.Auth.DialogGroupConfirm);
            case "message_new":
            {
                var message = Message.FromJson(new VkResponse(vkEvent.Object));
                Task.Run(() => _entryPoint.Enter(message));
                break;
            }
        }

        return Ok("ok");
    }
}
