namespace nng;

public readonly struct AdminRequest
{
    public readonly long Admin;
    private readonly DateTime _createdOn;
    public bool IsValid => (DateTime.Now - _createdOn).TotalMinutes < 5;

    public AdminRequest(long admin)
    {
        Admin = admin;
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
        return HashCode.Combine(Admin, _createdOn);
    }
}
