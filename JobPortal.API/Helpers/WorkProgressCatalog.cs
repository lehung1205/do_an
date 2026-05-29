namespace JobPortal.API.Helpers;

public static class WorkProgressCatalog
{
    public static readonly IReadOnlyList<string> AllowedStatuses = new[]
    {
        "confirmed",
        "in_progress",
        "pending_check",
        "completed",
        "cancelled"
    };

    public static bool IsValidStatus(string status) =>
        AllowedStatuses.Contains(status.Trim().ToLowerInvariant());

    public static bool IsLockedStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return false;
        }

        var s = status.Trim().ToLowerInvariant();
        return s is "completed" or "cancelled" or "terminated";
    }

    /// <summary>Employer/seeker may submit reviews after work is completed or cancelled.</summary>
    public static bool IsReviewableTerminalStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return false;
        }

        var s = status.Trim().ToLowerInvariant();
        return s is "completed" or "cancelled" or "terminated";
    }

    public static string GetTitle(string status) => FormatStatus(status);

    public static string FormatStatus(string status) => status.Trim().ToLowerInvariant() switch
    {
        "confirmed" => "Đã xác nhận nhận việc",
        "in_progress" => "Đang làm việc",
        "pending_check" => "Chờ nghiệm thu",
        "completed" => "Hoàn thành",
        "cancelled" => "Đã hủy",
        // Dữ liệu cũ (trước khi đổi giai đoạn part-time)
        "onboarding" => "Đã xác nhận nhận việc",
        "probation" => "Đang làm việc",
        "official" => "Chờ nghiệm thu",
        "terminated" => "Đã hủy",
        _ => status
    };

    public static string GetBadgeClass(string status) => status.Trim().ToLowerInvariant() switch
    {
        "confirmed" => "bg-info text-dark",
        "in_progress" => "bg-primary",
        "pending_check" => "bg-warning text-dark",
        "completed" => "bg-success",
        "cancelled" => "bg-danger",
        "onboarding" => "bg-info text-dark",
        "probation" => "bg-primary",
        "official" => "bg-warning text-dark",
        "terminated" => "bg-danger",
        _ => "bg-secondary"
    };

    /// <summary>Chuẩn hóa giá trị lọc tiến độ từ query (null = không lọc).</summary>
    public static bool TryNormalizeProgressFilter(string? value, out string? normalized)
    {
        normalized = null;
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        var filter = value.Trim().ToLowerInvariant();
        if (filter is "all")
        {
            return true;
        }

        if (filter is "none" or "no_progress")
        {
            normalized = "none";
            return true;
        }

        if (!IsValidStatus(filter))
        {
            return false;
        }

        normalized = filter;
        return true;
    }

    /// <summary>Các status DB khớp với một giá trị lọc (gồm dữ liệu cũ).</summary>
    public static IReadOnlyList<string> GetMatchingStatuses(string normalizedFilter) =>
        normalizedFilter.Trim().ToLowerInvariant() switch
        {
            "confirmed" => new[] { "confirmed", "onboarding" },
            "in_progress" => new[] { "in_progress", "probation" },
            "pending_check" => new[] { "pending_check", "official" },
            "completed" => new[] { "completed" },
            "cancelled" => new[] { "cancelled", "terminated" },
            _ => Array.Empty<string>()
        };
}
