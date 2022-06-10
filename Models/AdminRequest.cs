using nng_bot.Enums;

namespace nng_bot.Models;

public readonly struct AdminRequest
{
    public readonly long Admin;
    public readonly AdminRequestType Type;
    private readonly DateTime _createdOn;
    public bool IsValid => (DateTime.Now - _createdOn).TotalMinutes < 5;

    public AdminRequest(long admin, AdminRequestType type)
    {
        Admin = admin;
        Type = type;
        _createdOn = DateTime.Now;
    }

    public override bool Equals(object? obj)
    {
        if (obj is AdminRequest request) return Admin == request.Admin && _createdOn == request._createdOn;
        return false;
    }

    public static bool operator ==(AdminRequest left, AdminRequest right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(AdminRequest left, AdminRequest right)
    {
        return !(left == right);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Admin, (int) Type, _createdOn);
    }
}
