using nng.Enums;

namespace nng;

public struct UserProfile
{
    public long Id;
    public string Name;
    public long[] EditorGroups;
    public bool Banned;
    public bool BannedInBot;
    public BanPriority BanPriority;
    public DateTime? CreatedOn;
    public long Warnings;
    public bool Deleted;

    public bool CheckIfBanned()
    {
        return (Banned || BannedInBot) && !Deleted;
    }
}
