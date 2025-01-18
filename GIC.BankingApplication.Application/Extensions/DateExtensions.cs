using System.Globalization;

namespace GIC.BankingApplication.Application.Extensions;

public static class DateExtensions
{
    public static DateTime ParseAsUtcDate(this string dateStr)
    {
        if (string.IsNullOrWhiteSpace(dateStr))
            throw new ArgumentNullException(nameof(dateStr), "Date string cannot be null or empty.");

        if (!DateTime.TryParseExact(dateStr, "yyyyMMdd",
            CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
        {
            throw new FormatException($"Invalid date format: {dateStr}. Must be in YYYYMMdd format.");
        }

        return DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);
    }

    public static string ToYYYYMMddFormat(this DateTime date)
    {
        return date.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
    }
}
