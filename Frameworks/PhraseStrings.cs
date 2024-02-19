namespace nng_bot.Frameworks;

public static class PhraseStrings
{
    private static string SetBase(this string str, string id, object val)
    {
        return str.Replace(id, val.ToString(), StringComparison.CurrentCulture);
    }

    public static string SetId(this string str, object val)
    {
        return str.SetBase("{ID}", val);
    }

    public static string SetDate(this string str, object val)
    {
        return str.SetBase("{Date}", val);
    }

    public static string SetDateNewLine(this string str, object val)
    {
        return str.SetBase("{DateNewLine}", val);
    }

    public static string SetBan(this string str, object val)
    {
        return str.SetBase("{BAN}", val);
    }

    public static string SetEditorCounter(this string str, object val)
    {
        return str.SetBase("{EditorCounter}", val);
    }

    public static string SetEditor(this string str, object val)
    {
        return str.SetBase("{Editor}", val);
    }

    public static string SetBanStatus(this string str, object val)
    {
        return str.SetBase("{BanStatus}", val);
    }

    public static string SetStatus(this string str, object val)
    {
        return str.SetBase("{Status}", val);
    }

    public static string SetTime(this string str, object val)
    {
        return str.SetBase("{Time}", val);
    }

    public static string SetProfile(this string str, object val)
    {
        return str.SetBase("{Profile}", val);
    }

    public static string SetOldRequest(this string str, object val)
    {
        return str.SetBase("{OldRequest}", val);
    }

    public static string SetCount(this string str, object val)
    {
        return str.SetBase("{Count}", val);
    }

    public static string SetLog(this string str, object val)
    {
        return str.SetBase("{LOG}", val);
    }

    public static string SetName(this string str, object val)
    {
        return str.SetBase("{Name}", val);
    }

    public static string SetInfo(this string str, object val)
    {
        return str.SetBase("{Info}", val);
    }

    #region Stats

    public static string SetGroups(this string str, object val)
    {
        return str.SetBase("{Groups}", val);
    }

    public static string SetSlots(this string str, object val)
    {
        return str.SetBase("{Slots}", val);
    }

    public static string SetMembers(this string str, object val)
    {
        return str.SetBase("{Members}", val);
    }

    public static string SetMembersWithoutDuplicates(this string str, object val)
    {
        return str.SetBase("{MembersWithoutDuplicates}", val);
    }

    public static string SetManagers(this string str, object val)
    {
        return str.SetBase("{Managers}", val);
    }

    public static string SetManagersWithoutDuplicates(this string str, object val)
    {
        return str.SetBase("{ManagersWithoutDuplicates}", val);
    }

    #endregion
}
