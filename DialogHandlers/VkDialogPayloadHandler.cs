using nng_bot.API;
using nng_bot.Enums;
using nng_bot.Exceptions;
using nng_bot.Frameworks;
using nng_bot.Models;
using nng.Enums;
using Sentry;
using VkNet.Enums.SafetyEnums;
using VkNet.Exception;
using static nng_bot.API.KeyBoardFramework;
using static nng_bot.Configs.UsersConfigurationProcessor;

namespace nng;

public class VkDialogPayloadHandler
{
    private readonly CacheFramework _cacheFramework;
    private readonly VkController _controller;
    private readonly VkDialogHelper _helper;

    private readonly ILogger<VkDialogPayloadHandler> _logger;
    private readonly long _logUser;
    private readonly PhraseFramework _phraseFramework;

    private readonly OperationStatus _status;

    public VkDialogPayloadHandler(CacheFramework cacheFramework, VkController controller,
        PhraseFramework phraseFramework, VkDialogHelper helper, OperationStatus status,
        ILogger<VkDialogPayloadHandler> logger)
    {
        _cacheFramework = cacheFramework;
        _controller = controller;
        _phraseFramework = phraseFramework;
        _helper = helper;
        _status = status;
        _logger = logger;
        _logUser = EnvironmentConfiguration.GetInstance().Configuration.LogUser;
    }

    public void GiveEditor(long user)
    {
        var userProfile = _cacheFramework.LoadProfile(user);
        var priority = IfUserPrioritized(user);

        if (userProfile.CreatedOn != null && (DateTime.Now - userProfile.CreatedOn.Value).TotalDays < 180)
        {
            _controller.SendMessage(_phraseFramework.YourAccountIsTooYoung,
                GoToMenuButtons, user);
            return;
        }

        if (_helper.CheckIfCooldown(user)) return;

        var banned = userProfile.CheckIfBanned();

        switch (banned)
        {
            case true when !_status.UnbannedUsers.Contains(user):
                _controller.SendMessage(_phraseFramework.NoAvailableSlots,
                    GoToMenuButtons, user);
                return;
            case true when CacheFramework.BannedRequestsFull.Any(x =>
                x.UserId == user && x.Status is UnbanRequestStatus.Accepted):
                _controller.SendMessage(_phraseFramework.YourUnbanStillInProgress,
                    GoToMenuButtons, user);
                return;
        }

        if (userProfile.EditorGroups.Length >= EditorRestrictions && !priority)
        {
            _controller.SendMessage(_phraseFramework.LimitReached,
                GoToMenuButtons, user);
            return;
        }

        CacheGroup group;

        try
        {
            group = _helper.ChooseGroup(user, !priority);
        }
        catch (LessThanFiftySubs lessThanFiftySubs)
        {
            var actualGroup = _controller.GetGroupInfo(lessThanFiftySubs.Group.Id);
            if (actualGroup.Members.Contains(user))
            {
                _controller.SendMessage(_phraseFramework.NoAvailableSlots,
                    GoToMenuButtons, user);
                return;
            }

            _controller.SendMessage(_phraseFramework.LessThanFiftySubs(lessThanFiftySubs.Group),
                IveJoinedButtonsLessThanFiftySubs, user);
            return;
        }
        catch (NoAvailableGroups)
        {
            _controller.SendMessage(_phraseFramework.NoAvailableSlots,
                GoToMenuButtons, user);
            return;
        }

        _status.AddUserAskedForEditor(user);
        if (_status.UsersAskedForEditor[user] > 3)
        {
            _controller.SendMessage(_phraseFramework.YouAreOnCoolDown,
                GoToMenuButtons, user);
            return;
        }

        _status.AddEditorRequest(new EditorRequest(user, group.Id));
        _controller.SendMessage(_phraseFramework.PleaseJoinGroup(group), IveJoinedButtons, user);
    }

    public void Joined(long user)
    {
        var cachedGroups = CacheFramework.LoadCache();
        var priority = IfUserPrioritized(user);
        var joinedProfile = _cacheFramework.LoadProfile(user);

        EditorRequest request;
        try
        {
            request = _status.UsersToEditorGiving.First(x => x.User == user);
        }
        catch (InvalidOperationException)
        {
            _controller.SendMessage(_phraseFramework.YourRequestNoLongerValid,
                GoToMenuButtons, user);
            return;
        }
        catch (Exception e)
        {
            SentrySdk.CaptureException(e);
            var logToShow = $"{e.GetType()}: {e.Message}";
            _controller.SendMessage(_phraseFramework.Error(logToShow),
                GoToMenuButtons, user);
            return;
        }

        if (_helper.CheckIfCooldown(user))
        {
            _status.UsersToEditorGiving.Remove(request);
            return;
        }

        if (joinedProfile.CreatedOn != null && (DateTime.Now - joinedProfile.CreatedOn.Value).TotalDays < 180)
        {
            _controller.SendMessage(_phraseFramework.YourAccountIsTooYoung,
                GoToMenuButtons, user);
            _status.UsersToEditorGiving.Remove(request);
            return;
        }

        if (joinedProfile.CheckIfBanned())
        {
            _controller.SendMessage(_phraseFramework.TempUnavailable,
                GoToMenuButtons, user);
            _status.UsersToEditorGiving.Remove(request);
            return;
        }

        if (joinedProfile.EditorGroups.Length >= EditorRestrictions && !priority)
        {
            _controller.SendMessage(_phraseFramework.LimitReached,
                GoToMenuButtons, user);
            _status.UsersToEditorGiving.Remove(request);
            return;
        }

        if (!request.IsValid())
        {
            _controller.SendMessage(_phraseFramework.YourRequestNoLongerValid,
                GoToMenuButtons, user);
            _status.UsersToEditorGiving.Remove(request);
            return;
        }

        var groups = cachedGroups.Data.Where(cacheGroup => cacheGroup.Id == request.Group);
        if (!groups.Any())
        {
            _logger.LogCritical("Отсутвует запрашиваемая группа в кэше ({Group})", request.Group);
            _controller.SendMessage(_phraseFramework.Error("cache-error"),
                GoToMenuButtons, user);
            _status.UsersToEditorGiving.Remove(request);
            return;
        }

        Task.Delay(TimeSpan.FromSeconds(2)).GetAwaiter().GetResult();

        var data = _controller.GetGroupInfo(request.Group);
        if (!data.Members.Contains(request.User))
        {
            _controller.SendMessage(_phraseFramework.YouHaveNotJoinedClub,
                IveJoinedButtons, user);
            Task.Delay(TimeSpan.FromMilliseconds(500)).GetAwaiter().GetResult();
            _helper.ReplaceLongToGroup(ref cachedGroups, request);
            CacheFramework.SaveCache(cachedGroups);
            return;
        }

        string groupName;
        try
        {
            groupName = "@" + cachedGroups.Data.First(x => x.Id == request.Group).ShortName;
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
            _logger.LogWarning("Не удалось выдать редактора {User}: {Exception}: {Message}",
                user, e.GetType(), e.Message);
            var logToShow = $"{e.GetType()}: {e.Message}";
            _controller.SendMessage(_phraseFramework.Error(logToShow),
                GoToMenuButtons, user);
            _helper.ReplaceLongToGroup(ref cachedGroups, request);
            if (priority) _status.AddLimitlessUserCoolDown(user);
            else _status.CoolDownUsers.Add(user);
            CacheFramework.SaveCache(cachedGroups);
            return;
        }

        Task.Delay(TimeSpan.FromSeconds(1)).GetAwaiter().GetResult();
        _helper.ReplaceLongToGroup(ref cachedGroups, request);
        if (priority) _status.AddLimitlessUserCoolDown(user);
        else _status.CoolDownUsers.Add(user);
        CacheFramework.SaveCache(cachedGroups);

        _controller.SendMessage(
            _phraseFramework.EditorSuccess(groupName),
            GoToMenuButtons, user);
    }

    public void JoinedLessThanFiftySubs(long user)
    {
        var cache = CacheFramework.LoadCache();
        var group = Enumerable.Reverse(cache.Data).ToList().First();
        var updatedGroup = _controller.GetGroupInfo(group.Id);
        if (updatedGroup.Members.Contains(user))
        {
            _controller.SendMessage(_phraseFramework.EditorAfterFiftySubs, GoToMenuButtons,
                user);
            return;
        }

        _controller.SendMessage(_phraseFramework.YouHaveNotJoinedClub, IveJoinedButtonsLessThanFiftySubs,
            user);
    }

    public void UnBanMe(long user)
    {
        var userIsRequested = CacheFramework.TryGetBannedRequestFromFullList(user, out var request);

        switch (userIsRequested)
        {
            case true when !request.IsAvailableToReview():
                var message = _phraseFramework.GetUnbanRequestOverview(request);
                _controller.SendMessage(message, GoToMenuButtons, user);
                return;

            case true when request.IsAvailableToReview():
                var overview = _phraseFramework
                    .YouCanSendAnotherUnbanRequest(_phraseFramework.GetUnbanRequestOverview(request));
                _controller.SendMessage(overview, UnbanRequestConfirm, user);
                return;

            default:
                _controller.SendMessage(
                    _phraseFramework.AreYouSureYouWantToSendUnbanRequest, UnbanRequestConfirm,
                    user);
                return;
        }
    }

    public void SubmitUnBanRequest(long user)
    {
        if (!_cacheFramework.TryGetBannedPriority(user, out var priority))
        {
            _helper.SendErrorMessage(user);
            return;
        }

        if (priority is not BanPriority.Red and not BanPriority.Green and not BanPriority.White)
        {
            _helper.SendErrorMessage(user);
            return;
        }

        var isBanned = CacheFramework.TryGetBannedRequestFromFullList(user, out var request);

        switch (isBanned)
        {
            case true when request.IsAvailableToReview():
                CacheFramework.OverrideBannedRequest(request.UserId, new UnbanRequest(user, DateTime.Now));
                break;

            case true when !request.IsAvailableToReview():
                _helper.SendErrorMessage(user);
                return;

            default:
                CacheFramework.AddBannedRequest(new UnbanRequest(user, DateTime.Now));
                break;
        }

        _controller.SendMessage(_phraseFramework.UnbanRequestSent, GoToMenuButtons,
            user);
        _controller.SendMessage(_phraseFramework.NewUnbanRequest(user), null, _logUser);
    }
}
