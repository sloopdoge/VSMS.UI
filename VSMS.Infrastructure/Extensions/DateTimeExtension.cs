namespace VSMS.Infrastructure.Extensions;

public static class DateTimeExtension
{
    public static DateTime ConvertUtcToLocal(this DateTime utcDateTime, string timeZoneId)
    {
        var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc), tz);
    }
}