using nng_bot.Extensions;

namespace nng_bot.BackgroundServices;

public class CooldownFramework : IDisposable
{
    private static readonly TimeSpan RegularCooldown = TimeSpan.FromHours(6);
    private static readonly TimeSpan PriorityCooldown = TimeSpan.FromHours(4);

    private readonly Timer _cleanUpTimer;
    private readonly Dictionary<long, DateTime> _priorityUsersCooldown = new();

    private readonly Dictionary<long, DateTime> _regularUsersCooldown = new();

    public CooldownFramework()
    {
        _cleanUpTimer = new Timer(CleanUp, null, TimeSpan.FromHours(12), TimeSpan.FromHours(12));
    }

    public void Dispose()
    {
        _cleanUpTimer.Dispose();
        GC.SuppressFinalize(this);
    }

    private static bool TryFindUser(long user, IReadOnlyDictionary<long, DateTime> collection, out DateTime result)
    {
        if (collection.Any(x => x.Key.Equals(user)))
        {
            result = collection[user];
            return true;
        }

        result = DateTime.Now;
        return false;
    }

    private void CleanUp(object? state)
    {
        foreach (var (user, cooldownDate) in _regularUsersCooldown)
        {
            if (!cooldownDate.IsLongerThan(RegularCooldown)) continue;
            _regularUsersCooldown.Remove(user);
        }

        foreach (var (user, cooldownDate) in _priorityUsersCooldown)
        {
            if (!cooldownDate.IsLongerThan(PriorityCooldown)) continue;
            _priorityUsersCooldown.Remove(user);
        }
    }

    private bool HasPriorityCooldown(long id)
    {
        return TryFindUser(id, _priorityUsersCooldown, out var dateTime) && !dateTime.IsLongerThan(PriorityCooldown);
    }

    private bool HasRegularCooldown(long id)
    {
        return TryFindUser(id, _regularUsersCooldown, out var dateTime) && !dateTime.IsLongerThan(RegularCooldown);
    }

    public void AddRegularCooldown(long user)
    {
        _regularUsersCooldown[user] = DateTime.Now;
    }

    public void AddPriorityCooldown(long user)
    {
        _priorityUsersCooldown[user] = DateTime.Now;
    }

    public bool HasCooldown(long id)
    {
        return HasPriorityCooldown(id) || HasRegularCooldown(id);
    }
}
