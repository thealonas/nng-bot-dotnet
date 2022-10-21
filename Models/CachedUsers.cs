using Newtonsoft.Json;
using nng.Models;

namespace nng_bot.Models;

public class CachedUsers
{
    public CachedUsers(List<UserModel> bannedUsers, List<UserModelShort> priorityUsers, DateTime createdOn)
    {
        BannedUsers = bannedUsers;
        CreatedOn = createdOn;
        PriorityUsers = priorityUsers;
    }

    [JsonProperty("bnnd")] public List<UserModel> BannedUsers { get; set; }

    [JsonProperty("thx")] public List<UserModelShort> PriorityUsers { get; set; }

    [JsonProperty("datetime")] public DateTime CreatedOn { get; set; }
}
