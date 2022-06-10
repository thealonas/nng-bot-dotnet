namespace nng_bot.Models;

[Serializable]
public struct UsersConfig
{
    public readonly List<long> BannedUsers;
    public readonly List<long> PriorityUsers;
    public readonly List<long> AdminUsers;
    public int EditorRestriction { get; set; }
    public int GroupManagersCelling { get; set; }

    public UsersConfig()
    {
        BannedUsers = new List<long>();
        PriorityUsers = new List<long>();
        AdminUsers = new List<long>();
        GroupManagersCelling = 0;
        EditorRestriction = 0;
    }
}
