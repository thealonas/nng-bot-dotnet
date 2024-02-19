using nng_bot;
using nng_bot.API;
using nng_bot.BackgroundServices;
using nng_bot.Exceptions;
using nng_bot.Extensions;
using nng_bot.Frameworks;
using nng_bot.Models;
using nng.DatabaseModels;
using nng.DatabaseProviders;
using nng.Extensions;
using Sentry;
using VkNet.Enums.SafetyEnums;
using VkNet.Exception;
using static nng_bot.Frameworks.KeyBoardFramework;
using User = nng.DatabaseModels.User;

namespace nng;

public class VkDialogPayloadHandler
{
    private readonly VkController _controller;
    private readonly CooldownFramework _cooldownFramework;
    private readonly GroupsDatabaseProvider _groups;
    private readonly GroupStatsDatabaseProvider _groupStats;
    private readonly VkDialogHelper _helper;

    private readonly ILogger<VkDialogPayloadHandler> _logger;

    private readonly Settings _settings;
    private readonly OperationStatus _status;
    private readonly UserFramework _userFramework;

    public VkDialogPayloadHandler(VkController controller, VkDialogHelper helper,
        OperationStatus status, ILogger<VkDialogPayloadHandler> logger, SettingsDatabaseProvider settings,
        UserFramework userFramework, GroupsDatabaseProvider groups, GroupStatsDatabaseProvider groupStats,
        CooldownFramework cooldownFramework)
    {
        _controller = controller;
        _helper = helper;
        _status = status;
        _logger = logger;
        _userFramework = userFramework;
        _groups = groups;
        _groupStats = groupStats;
        _cooldownFramework = cooldownFramework;

        if (!settings.Collection.TryGetById("main", out var mainSettings))
            throw new ArgumentException("Invalid settings", nameof(settings));

        _settings = mainSettings;
    }

    public void GiveEditor(long id, User? user)
    {
        if (_helper.CheckIfCooldown(id)) return;

        if (user is not null && (user.Groups?.Count >= _settings.EditorRestriction && !user.HasPriority() || user.Banned))
        {
            _controller.SendMessage(PhraseFramework.LimitReached, GoToMenuButtons, id,
                false);
            return;
        }

        GroupInfo group;

        try
        {
            if (!_status.GroupsHistory.ContainsKey(id))
            {
                group = _helper.ChooseGroup(user);
                _status.GroupsHistory[id] = group.GroupId;
            }
            else
            {
                group = _helper.FindGroupById(_status.GroupsHistory[id]);
            }
        }
        catch (LessThanFiftySubs lessThanFiftySubs)
        {
            var actualGroup = _controller.VkFramework.GetGroupData(lessThanFiftySubs.Group.GroupId);
            if (actualGroup.AllUsers.Any(x => x.Id.Equals(id)))
            {
                _controller.SendMessage(PhraseFramework.NoAvailableSlots, GoToMenuButtons, id);
                return;
            }

            _controller.SendMessage(PhraseFramework.LessThanFiftySubs(lessThanFiftySubs.Group.ScreenName),
                IveJoinedButtonsLessThanFiftySubs, id);
            return;
        }
        catch (NoAvailableGroups)
        {
            _controller.SendMessage(PhraseFramework.NoAvailableSlots,
                GoToMenuButtons, id);
            return;
        }

        if (_status.UsersAskedForEditor.TryGetValue(id, out var requestCount) && requestCount > 3)
        {
            _controller.SendMessage(PhraseFramework.YouAreOnCoolDown, GoToMenuButtons, id);
            return;
        }

        _status.AddEditorRequest(new EditorRequest(id, group.GroupId));
        _controller.SendMessage(PhraseFramework.PleaseJoinGroup(group.ScreenName), IveJoinedButtons, id);
    }

    private void AddGroupToUser(User? user, long userId, long group)
    {
        if (user is null)
        {
            user = new User
            {
                Admin = false,
                Thanks = false,
                App = true,
                Banned = false,
                Groups = new List<long>(),
                UserId = userId,
                LastUpdated = DateTime.Now
            };

            var vkUser = _controller.VkFramework.GetUser(userId);
            user.Name = $"{vkUser.FirstName} {vkUser.LastName}";
        }

        user.App = true;
        user.Groups?.Add(group);

        _userFramework.UsersDatabase.Collection.Insert(user);
    }

    private void AddCooldown(User? user, long userId)
    {
        if (user is null || !user.Thanks)
        {
            _cooldownFramework.AddRegularCooldown(userId);
            return;
        }

        _cooldownFramework.AddPriorityCooldown(user.UserId);
    }

    private void UpdateGroupInDb(long groupId, Models.GroupData data)
    {
        if (!_groupStats.Collection.TryGetById(groupId.ToString(), out var dbGroup))
        {
            _logger.LogError("Не удалось найти группу {Group} в базе данных", groupId);
            return;
        }

        dbGroup.Managers = data.ManagerCount;
        dbGroup.Members = data.Count;

        _groupStats.Collection.Insert(dbGroup);

        _logger.LogInformation("Обновили сообщество {Group} в базе данных статистики", groupId);
    }

    public void Joined(long user)
    {
        EditorRequest request;
        try
        {
            request = _status.UsersToEditorGiving.First(x => x.User == user);
        }
        catch (InvalidOperationException)
        {
            _controller.SendMessage(PhraseFramework.YourRequestNoLongerValid,
                GoToMenuButtons, user);
            return;
        }
        catch (Exception e)
        {
            SentrySdk.CaptureException(e);
            var logToShow = $"{e.GetType()}: {e.Message}";
            _controller.SendMessage(PhraseFramework.Error(logToShow),
                GoToMenuButtons, user);
            return;
        }

        if (_helper.CheckIfCooldown(user))
        {
            _status.UsersToEditorGiving.Remove(request);
            _controller.SendMessage(PhraseFramework.YouAreOnCoolDown, GoToMenuButtons, user);
            return;
        }

        if (!request.IsValid())
        {
            _controller.SendMessage(PhraseFramework.YourRequestNoLongerValid,
                GoToMenuButtons, user);
            _status.UsersToEditorGiving.Remove(request);
            return;
        }

        var data = _controller.VkFramework.GetGroupData(request.Group);

        UpdateGroupInDb(request.Group, data);

        if (!data.AllUsers.Any(x => x.Id.Equals(user)))
        {
            _controller.SendMessage(PhraseFramework.YouHaveNotJoinedClub, IveJoinedButtons, user);
            return;
        }

        User? profile;
        try
        {
            profile = _userFramework.GetById(user);
        }
        catch (Exception)
        {
            profile = null;
        }

        string groupName;
        try
        {
            groupName = "@" + _groups.Collection.ToList().First(x => x.GroupId.Equals(request.Group)).ScreenName;
        }
        catch (InvalidOperationException)
        {
            groupName = $"@club{request.Group}";
        }

        _status.UsersToEditorGiving.Remove(request);

        try
        {
            _controller.EditManager(user, request.Group, ManagerRole.Editor);
        }
        catch (VkApiException e)
        {
            _logger.LogWarning("Не удалось выдать редактора {User}: {Exception}: {Message}", user, e.GetType(),
                e.Message);

            var logToShow = $"{e.GetType()}: {e.Message}";

            _controller.SendMessage(PhraseFramework.Error(logToShow), GoToMenuButtons, user);
            AddCooldown(profile, user);

            return;
        }

        _status.AddUserAskedForEditor(user);
        _status.GroupsHistory.Remove(user);

        AddCooldown(profile, user);
        AddGroupToUser(profile, user, request.Group);

        _controller.SendMessage(PhraseFramework.EditorSuccess(groupName), GoToMenuButtons, user);
        _controller.SendSticker(user, 60785);
    }

    private long? GetGroupForJoinedLess50()
    {
        var groups = _groupStats.Collection.ToList();

        var groupLessThan50 = groups.Where(x => x.Members < 50).ToList();

        if (groupLessThan50.Any()) return groupLessThan50.First().GroupId;

        var groupLessThan100 = groups.Where(x => x.Members < 100).ToList();

        if (groupLessThan100.Any()) return groupLessThan100.First().GroupId;

        return null;
    }

    public void JoinedLessThanFiftySubs(long user)
    {
        var group = GetGroupForJoinedLess50();

        if (group is null)
        {
            _controller.SendMessage(PhraseFramework.Error("Cannot find target group"), GoToMenuButtons, user);
            return;
        }

        var groupData = _controller.VkFramework.GetGroupData(group.Value);

        if (groupData.AllUsers.Any(x => x.Id.Equals(user)))
        {
            _controller.SendMessage(PhraseFramework.EditorAfterFiftySubs, GoToMenuButtons,
                user);
            return;
        }

        _controller.SendMessage(PhraseFramework.YouHaveNotJoinedClub, IveJoinedButtonsLessThanFiftySubs,
            user);
    }
}
