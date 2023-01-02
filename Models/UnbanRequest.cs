using nng_bot.Enums;

namespace nng_bot.Models;

public sealed class UnbanRequest : IEquatable<UnbanRequest>
{
    public UnbanRequest(long userId, DateTime createdOn)
    {
        UserId = userId;
        CreatedOn = createdOn;
        Status = UnbanRequestStatus.Pending;
        IsWatched = false;
    }

    public DateTime CreatedOn { get; }
    public long UserId { get; }
    public bool IsWatched { get; set; }
    public UnbanRequestStatus Status { get; set; }

    public bool Equals(UnbanRequest? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return UserId == other.UserId && IsWatched == other.IsWatched && Status == other.Status;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not UnbanRequest other)
            return false;
        return other.UserId == UserId && other.CreatedOn == CreatedOn;
    }

    public bool IsAvailableToReview()
    {
        return (DateTime.Now - CreatedOn).TotalDays >= 30;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(CreatedOn, UserId);
    }
}
