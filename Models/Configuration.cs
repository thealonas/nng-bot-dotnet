using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Redis.OM.Modeling;

namespace nng_bot.Models;

[Document(StorageType = StorageType.Json, Prefixes = new[] {"settings:bot"}, IndexName = "settings:bot")]
public class Configuration
{
    public Configuration(long groupId, string groupToken, string groupSecret, string groupConfirm)
    {
        GroupId = groupId;
        GroupToken = groupToken;
        GroupSecret = groupSecret;
        GroupConfirm = groupConfirm;
    }

    [Indexed(PropertyName = "group_id")]
    [JsonProperty("group_id")]
    [JsonPropertyName("group_id")]
    public long GroupId { get; set; }

    [Indexed(PropertyName = "group_token")]
    [JsonProperty("group_token")]
    [JsonPropertyName("group_token")]
    public string GroupToken { get; set; }

    [Indexed(PropertyName = "group_secret")]
    [JsonProperty("group_secret")]
    [JsonPropertyName("group_secret")]
    public string GroupSecret { get; set; }

    [Indexed(PropertyName = "group_confirm")]
    [JsonProperty("group_confirm")]
    [JsonPropertyName("group_confirm")]
    public string GroupConfirm { get; set; }
}
