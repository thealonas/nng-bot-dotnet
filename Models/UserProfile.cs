using nng.Enums;

namespace nng_bot.Models;

public struct UserProfile
{
    public long Id { get; init; }
    public string Name { get; set; }
    public long[] EditorGroups { get; set; }
    public bool Banned { get; set; }
    public bool BannedInBot { get; set; }
    public BanPriority BanPriority { get; set; }
    public DateTime? CreatedOn { get; init; }
    public long Warnings { get; set; }
    public bool Deleted { get; set; }
}
