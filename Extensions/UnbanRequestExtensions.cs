using nng_bot.Enums;

namespace nng_bot.Extensions;

public static class UnbanRequestStatusExtensions
{
    public static string GetName(this UnbanRequestStatus status)
    {
        return status switch
        {
            UnbanRequestStatus.Pending => "На рассмотрении",
            UnbanRequestStatus.Rejected => "Отклонено",
            UnbanRequestStatus.Accepted => "Принято",
            _ => "Unknown"
        };
    }
}
