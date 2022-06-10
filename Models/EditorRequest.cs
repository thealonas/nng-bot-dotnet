namespace nng_bot.Models;

public readonly struct EditorRequest
{
    public EditorRequest(long user, long group)
    {
        User = user;
        Group = group;
        Time = DateTime.Now;
    }

    public long User { get; }
    public long Group { get; }
    private DateTime Time { get; }

    public bool IsValid()
    {
        return (DateTime.Now - Time).TotalMinutes <= 5;
    }
}
