﻿namespace nng_bot.Models;

[Serializable]
public struct UsersConfig
{
    public readonly List<long> BannedUsers;
    public readonly List<long> PriorityUsers;
    public readonly List<long> AdminUsers;
    public int EditorRestriction { get; set; }
    public int GroupManagersCeiling { get; set; }

    public UsersConfig()
    {
        BannedUsers = new List<long>();
        PriorityUsers = new List<long>();
        AdminUsers = new List<long>();
        GroupManagersCeiling = 0;
        EditorRestriction = 0;
    }
}
