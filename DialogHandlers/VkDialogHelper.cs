using nng_bot.API;
using nng_bot.BackgroundServices;
using nng_bot.Exceptions;
using nng_bot.Extensions;
using nng_bot.Frameworks;
using nng.DatabaseModels;
using nng.DatabaseProviders;
using static nng_bot.Frameworks.KeyBoardFramework;

namespace nng_bot;

public class VkDialogHelper
{
    private readonly CooldownFramework _cooldownFramework;
    private readonly GroupsDatabaseProvider _groupsDatabaseProvider;
    private readonly GroupStatsDatabaseProvider _groupStatsDatabaseProvider;

    private readonly VkController _vkController;

    public VkDialogHelper(VkController vkController,
        GroupStatsDatabaseProvider groupStatsDatabaseProvider, GroupsDatabaseProvider groupsDatabaseProvider,
        CooldownFramework cooldownFramework)
    {
        _vkController = vkController;
        _groupStatsDatabaseProvider = groupStatsDatabaseProvider;
        _groupsDatabaseProvider = groupsDatabaseProvider;
        _cooldownFramework = cooldownFramework;
    }

    public bool CheckIfCooldown(long user)
    {
        if (!_cooldownFramework.HasCooldown(user)) return false;

        _vkController.SendMessage(PhraseFramework.YouAreOnCoolDown,
            GoToMenuButtons, user);

        return true;
    }

    private static bool IsManager(User? user, long group)
    {
        return user?.Groups is not null && user.Groups.Any(x => x.Equals(group));
    }

    private GroupInfo GetById(long id)
    {
        return _groupsDatabaseProvider.Collection.ToList().First(x => x.GroupId == id);
    }

    private bool TryChooseMinorGroup(IEnumerable<GroupStats> groups, User? user, out LessThanFiftySubs exception)
    {
        exception = new LessThanFiftySubs(new GroupInfo());
        try
        {
            var potentialGroup = groups.Where(x =>
                x.Members < 50 && !IsManager(user, x.GroupId) && x.Managers < 100).GetRandom();
            exception = new LessThanFiftySubs(GetById(potentialGroup.GroupId));
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private bool TryChooseMajorGroup(IEnumerable<GroupStats> groups, User? user, out GroupInfo group)
    {
        group = new GroupInfo();
        try
        {
            var potentialGroup = groups.Where(x =>
                    x.Members is >= 50 and <= 100 && !IsManager(user, x.GroupId) && x.Managers < 100)
                .GetRandom();
            group = GetById(potentialGroup.GroupId);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private bool TryChooseUsualGroup(IEnumerable<GroupStats> groups, User? user, out GroupInfo group)
    {
        group = new GroupInfo();
        try
        {
            var usualGroup = groups.Where(x =>
                x.Members > 50 && !IsManager(user, x.GroupId) && x.Managers < 100).GetRandom();
            group = GetById(usualGroup.GroupId);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public GroupInfo ChooseGroup(User? user)
    {
        var groups = _groupStatsDatabaseProvider.Collection.ToList();

        var priority = user.HasPriority();

        if (!priority && TryChooseMinorGroup(groups, user, out var exception)) throw exception;

        if (TryChooseMajorGroup(groups, user, out var potentialGroup)) return potentialGroup;

        if (TryChooseUsualGroup(groups, user, out potentialGroup)) return potentialGroup;

        throw new NoAvailableGroups();
    }

    public static string GetStartMenuKeyboard(User? user)
    {
        if (user is null) return StartButtons;
        return user.Banned ? RestrictedStartButtons : StartButtons;
    }

    public GroupInfo FindGroupById(long id)
    {
        return _groupsDatabaseProvider.Collection.ToList().First(x => x.GroupId.Equals(id));
    }
}
