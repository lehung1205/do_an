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
}
