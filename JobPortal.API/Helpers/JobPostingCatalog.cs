namespace JobPortal.API.Helpers;

public static class JobPostingCatalog
{
    public const int PendingAutoApproveHours = 24;

    public static readonly TimeSpan PendingAutoApproveDelay = TimeSpan.FromHours(PendingAutoApproveHours);

    public const string Pending = "pending";
    public const string Recruiting = "recruiting";
    public const string Rejected = "rejected";
    public const string Closed = "closed";
    public const string Draft = "draft";

    public static bool IsPubliclyVisible(string status) =>
        string.Equals(status, Recruiting, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(status, Closed, StringComparison.OrdinalIgnoreCase);

    public static bool IsValidStatus(string status) =>
        status.Trim().ToLowerInvariant() is Pending or Recruiting or Rejected or Closed or Draft;
}
