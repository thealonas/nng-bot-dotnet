using nng;
using nng.DatabaseModels;
using nng.DatabaseProviders;
using nng.Enums;
using nng.Extensions;

namespace nng_bot.Frameworks;

public partial class PhraseFramework
{
    private readonly GroupsDatabaseProvider _groupsProvider;

    public PhraseFramework(SettingsDatabaseProvider settingsProvider, GroupsDatabaseProvider groupsProvider)
    {
        _groupsProvider = groupsProvider;

        if (!settingsProvider.Collection.TryGetById("main", out var settings))
            throw new ArgumentException(nameof(settingsProvider));

        Settings = settings;
    }

    private Settings Settings { get; }

    private static string FillBannedStatus(bool banned, bool hasBannedInfo)
    {
        return banned ? "💥 Статус блокировки: Заблокирован" :
            hasBannedInfo ? "💥 Статус блокировки: Разблокирован 🎉" : "💥 Статус блокировки: Не заблокирован 🎉";
    }

    private static string FillBannedInfo(BannedInfo info)
    {
        var result = $"\n📄 Причина: {((BanPriority) info.Priority).GetName()}\n";

        if (info.Complaint is null && info.Date is null && info.GroupId is null) return result;

        result = $"\n♨️ Сведения о блокировке:{result}";

        if (info.Complaint is not null) result += $"📢 Жалоба от: @id{info.Complaint.Value}\n";

        if (info.Date is not null) result += $"⏱ Дата нарушения: {info.Date.Value:dd.MM.yyyy}\n";

        if (info.GroupId is not null) result += $"👥 Группа: @club{info.GroupId.Value}\n";

        return result.TrimEnd('\n');
    }

    private string FillGroups(IReadOnlyCollection<long> groups)
    {
        var groupsInfo = _groupsProvider.Collection.ToList();
        return groups.Any()
            ? string.Join(", ", groups.Select(x =>
            {
                string group;
                try
                {
                    group = "@" + groupsInfo.First(y => y.GroupId == x).ScreenName;
                }
                catch (InvalidOperationException)
                {
                    group = $"@club{x}";
                }

                return group;
            }))
            : "❌";
    }

    public string FormProfile(User user, DateTime? registeredOn)
    {
        var banStatus = FillBannedStatus(user.Banned, user.BannedInfo is not null);

        var ban = string.Empty;

        if (user.BannedInfo is not null) ban += FillBannedInfo(user.BannedInfo);

        var groups = user.Groups ?? new List<long>();

        string editor;
        string editorCounter;

        if (user.Banned)
        {
            editor = "❌";
            editorCounter = string.Empty;
        }
        else
        {
            editor = FillGroups(groups);

            var limit = user.Thanks || user.Admin ? "∞" : Settings.EditorRestriction.ToString();
            editorCounter = groups.Any() ? $" ({groups.Count}/{limit})" : string.Empty;
        }

        if (!groups.Any()) editor = YouHaveNoEditor;
        string phrase;

        if (registeredOn is null)
        {
            phrase = Profile(user.Name, user.UserId, string.Empty, string.Empty, ban, banStatus,
                editor, editorCounter);
            return phrase;
        }

        var createdOn = $"⏱ Дата регистрации: {registeredOn:dd.MM.yyyy}";
        phrase = Profile(user.Name, user.UserId, createdOn, "\n", ban, banStatus, editor, editorCounter);
        return phrase;
    }

    public string FormBlankProfile(long user, string name, DateTime? registeredOn)
    {
        var ban = string.Empty;

        ban += FillBannedStatus(false, false);

        const string editor = "❌";
        var editorCounter = string.Empty;

        string phrase;

        if (registeredOn is null)
        {
            phrase = Profile(name, user, string.Empty, string.Empty, string.Empty, ban, editor, editorCounter);
            return phrase;
        }

        var createdOn = $"⏱ Дата регистрации: {registeredOn:dd.MM.yyyy}";
        phrase = Profile(name, user, createdOn, "\n", string.Empty, ban, editor, editorCounter);
        return phrase;
    }
}
