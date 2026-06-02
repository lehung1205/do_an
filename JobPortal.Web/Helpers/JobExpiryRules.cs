namespace JobPortal.Web.Helpers;

public static class JobExpiryRules
{
    public const int DefaultExpiryMonths = 2;
    public const int MaxExpiryMonths = 12;

    public static DateTime DefaultExpiryDateForForm() =>
        DateTime.UtcNow.Date.AddMonths(DefaultExpiryMonths);

    public static DateTime NormalizeExpiryDateUtc(DateTime value)
    {
        var date = value.Date;
        return DateTime.SpecifyKind(date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
    }

    public static string? ValidateExpiryDate(DateTime expiryDate)
    {
        var selectedDay = expiryDate.Date;
        var today = DateTime.UtcNow.Date;

        if (selectedDay < today)
        {
            return "Ngày hết hạn không được trước hôm nay.";
        }

        if (selectedDay > today.AddMonths(MaxExpiryMonths))
        {
            return $"Ngày hết hạn không được quá {MaxExpiryMonths} tháng kể từ hôm nay.";
        }

        return null;
    }
}
