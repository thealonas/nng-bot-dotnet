using Newtonsoft.Json;

namespace nng_bot.Models;

public class CacheData
{
    public CacheData(List<CacheGroup> data, DateTime createdOn)
    {
        Data = data;
        CreatedOn = createdOn;
    }

    [JsonProperty("data")] public List<CacheGroup> Data { get; set; }

    [JsonProperty("datetime")] public DateTime CreatedOn { get; set; }
}
