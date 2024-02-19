using Newtonsoft.Json;

namespace nng;

public class GroupData
{
    public GroupData(List<long> allUsers, List<long> managers, string shortName, int count, int managerCount)
    {
        AllUsers = allUsers;
        Managers = managers;
        ShortName = shortName;
        Count = count;
        ManagerCount = managerCount;
    }

    [JsonProperty("users")] public List<long> AllUsers { get; set; }

    [JsonProperty("managers")] public List<long> Managers { get; set; }

    [JsonProperty("screen_name")] public string ShortName { get; set; }

    [JsonProperty("count")] public int Count { get; set; }

    [JsonProperty("manager_count")] public int ManagerCount { get; set; }
}
