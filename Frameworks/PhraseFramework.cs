using nng;
using nng_bot.Configs;
using nng_bot.Enums;
using nng_bot.Extensions;
using nng_bot.Models;

namespace nng_bot.Frameworks;

public partial class PhraseFramework
{
    public PhraseFramework(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    private IConfiguration Configuration { get; }


    public string FormProfile(UserProfile profile, bool limitless)
    {
        var cache = CacheFramework.LoadCache();
        var groupList = profile.EditorGroups.ToList();

        var ban = string.Empty;
        if (!profile.Banned || profile.Deleted)
        {
            ban = "💥 Статус блокировки: Не заблокирован 🎉";
        }
        else
        {
            if (profile.Warnings > 0) ban += $"‼️ Количетсво жалоб: {profile.Warnings}\n";
            if (profile.BannedInBot) ban += "💥 Статус блокировки: Заблокирован — в боте";
            else ban += $"💥 Статус блокировки: Заблокирован — {profile.BanPriority.GetName()}";
        }

        string editor;
        string editorCounter;
        if (profile.Banned && !profile.Deleted)
        {
            editor = "❌";
            editorCounter = string.Empty;
        }
        else
        {
            editor = groupList.Any()
                ? string.Join(", ", groupList.Select(x =>
                {
                    string group;
                    try
                    {
                        group = "@" + cache.Data.First(cacheGroup => cacheGroup.Id == x).ShortName;
                    }
                    catch (InvalidOperationException)
                    {
                        group = $"@club{x}";
                    }

                    return group;
                }))
                : "❌";

            var limit = limitless ? "∞" : UsersConfigurationProcessor.EditorRestrictions.ToString();
            editorCounter = groupList.Any() ? $" ({groupList.Count}/{limit})" : string.Empty;
        }

        if (!groupList.Any()) editor = YouHaveNoEditor;
        string phrase;
        if (profile.CreatedOn == null)
        {
            phrase = Profile(profile.Name, profile.Id, string.Empty, string.Empty, ban, editor, editorCounter);
            return phrase;
        }

        var createdOn = $"⏱ Дата регистрации: {profile.CreatedOn.Value:dd.MM.yyyy}";
        phrase = Profile(profile.Name, profile.Id, createdOn, "\n", ban, editor, editorCounter);
        return phrase;
    }

    public string GetUnbanRequestOverview(UnbanRequest request)
    {
        if (request == null) throw new NullReferenceException(nameof(request));
        return UnbanRequestOverview(request.CreatedOn.ToString("dd.MM.yyyy HH:mm:ss"), request.Status.GetName());
    }

    private string GetPhrase(string name)
    {
        var target = Configuration[$"Phrases:{name}"];
        return target ?? string.Empty;
    }
}
