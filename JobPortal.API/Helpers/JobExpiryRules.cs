using JobPortal.API.Exceptions;

namespace JobPortal.API.Helpers;

public static class JobExpiryRules
{
    public const int DefaultExpiryMonths = 2;
    public const int MaxExpiryMonths = 12;

    public static DateTime DefaultExpiryDateUtc() =>
        DateTime.UtcNow.Date.AddMonths(DefaultExpiryMonths);

    /// <summary>End of the selected calendar day (UTC).</summary>
    public static DateTime NormalizeExpiryDateUtc(DateTime value)
    {
        var date = value.Date;
        return DateTime.SpecifyKind(date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
    }

    public static void ValidateExpiryDateUtc(DateTime expiryDate)
    {
        var selectedDay = expiryDate.Date;
        var today = DateTime.UtcNow.Date;

        if (selectedDay < today)
        {
            throw new BadRequestException("Ngày hết hạn không được trước hôm nay.");
        }

        if (selectedDay > today.AddMonths(MaxExpiryMonths))
        {
            throw new BadRequestException($"Ngày hết hạn không được quá {MaxExpiryMonths} tháng kể từ hôm nay.");
        }
    }
}
