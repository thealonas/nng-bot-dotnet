using Newtonsoft.Json;

namespace nng_bot.Models;

public struct CacheGroup
{
    [JsonProperty("id")] public long Id { get; init; }

    [JsonProperty("screen_name")] public string ShortName { get; set; }

    [JsonProperty("members")] public List<long> Members { get; set; }

    [JsonProperty("members_count")] public int MembersTotalCount { get; set; }

    [JsonProperty("manager_count")] public int ManagerTotalCount { get; set; }

    [JsonProperty("managers")] public List<long> Managers { get; set; }

    public override string ToString()
    {
        return $"@{ShortName}";
    }
}
