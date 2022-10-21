namespace nng_bot.Models;

[Serializable]
public struct UsersConfig
{
    public List<long> AdminUsers { get; set; }
    public int EditorRestriction { get; set; }
    public int GroupManagersCeiling { get; set; }

    public UsersConfig()
    {
        AdminUsers = new List<long>();
        GroupManagersCeiling = 0;
        EditorRestriction = 0;
    }
}
