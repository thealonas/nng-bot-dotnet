using Newtonsoft.Json;
using nng.Models;

namespace nng_bot.Models;

public class CacheData
{
    public CacheData(List<CacheGroup> data, List<UserModel> bannedUsers, DateTime createdOn)
    {
        Data = data;
        BannedUsers = bannedUsers;
        CreatedOn = createdOn;
    }

    [JsonProperty("data")] public List<CacheGroup> Data { get; set; }

    [JsonProperty("users")] public List<UserModel> BannedUsers { get; set; }

    [JsonProperty("datetime")] public DateTime CreatedOn { get; set; }
}
