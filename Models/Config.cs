namespace nng_bot.Models;

public class Config
{
    public Config(string dataUrl, bool editorGrantEnabled, long logUser, AuthConfig auth, CacheConfig cache)
    {
        DataUrl = dataUrl;
        EditorGrantEnabled = editorGrantEnabled;
        LogUser = logUser;
        Auth = auth;
        Cache = cache;
    }

    public string DataUrl { get; }
    public bool EditorGrantEnabled { get; }
    public long LogUser { get; }
    public AuthConfig Auth { get; }
    public CacheConfig Cache { get; }
}

public class AuthConfig
{
    public AuthConfig(string userToken, long dialogGroupId, string dialogGroupToken, string dialogGroupSecret,
        string dialogGroupConfirm)
    {
        UserToken = userToken;
        DialogGroupId = dialogGroupId;
        DialogGroupToken = dialogGroupToken;
        DialogGroupSecret = dialogGroupSecret;
        DialogGroupConfirm = dialogGroupConfirm;
    }

    public string UserToken { get; }
    public long DialogGroupId { get; }
    public string DialogGroupToken { get; }
    public string DialogGroupSecret { get; }
    public string DialogGroupConfirm { get; }
}

public class CacheConfig
{
    public CacheConfig(bool updateAtStart, int updatePerHours)
    {
        UpdateAtStart = updateAtStart;
        UpdatePerHours = updatePerHours;
    }

    public bool UpdateAtStart { get; }
    public int UpdatePerHours { get; }
}
