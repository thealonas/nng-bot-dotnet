using nng.DatabaseProviders;
using nng.Extensions;
using nng.VkFrameworks;

namespace nng_bot.Providers;

public class VkFrameworkProvider
{
    private readonly ILogger<VkFrameworkProvider> _logger;
    private readonly TokensDatabaseProvider _tokens;

    private VkFramework _vkFramework;

    public VkFrameworkProvider(TokensDatabaseProvider tokens, ILogger<VkFrameworkProvider> logger)
    {
        _tokens = tokens;
        _logger = logger;
        CurrentUser = 0;
        _vkFramework = new VkFramework();
    }

    private long CurrentUser { get; set; }

    public VkFramework VkFramework
    {
        get
        {
            UpdateToken();
            return _vkFramework;
        }
    }

    private void UpdateToken()
    {
        var token = _tokens.GetTokenWithPermission("bot");
        if (token.UserId.Equals(CurrentUser)) return;

        _logger.LogInformation("Новый токен на пользователя {User} с правами {Perms}",
            token.UserId, token.Permissions);
        CurrentUser = token.UserId;
        _vkFramework = new VkFramework(token.Token);
    }
}
