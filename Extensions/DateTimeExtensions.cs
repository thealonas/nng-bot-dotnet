namespace nng_bot.Extensions;

public static class DateTimeExtensions
{
    public static bool IsLongerThan(this DateTime time, TimeSpan span)
    {
        var totalSeconds = (DateTime.Now - time).TotalSeconds;
        var spanTotalSeconds = span.TotalSeconds;
        return totalSeconds > spanTotalSeconds;
    }
}
