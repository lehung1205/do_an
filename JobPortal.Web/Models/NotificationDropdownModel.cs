namespace JobPortal.Web.Models;

public class NotificationDropdownModel
{
    public string Variant { get; set; } = "seeker";

    public int UnreadCount { get; set; }

    public bool IsActive { get; set; }

    public IReadOnlyList<Helpers.NotificationDisplayHelper.NotificationGroupModel> Groups { get; set; } =
        Array.Empty<Helpers.NotificationDisplayHelper.NotificationGroupModel>();

    public string AntiForgeryFieldName { get; set; } = "__RequestVerificationToken";

    public string AntiForgeryRequestToken { get; set; } = string.Empty;

    public string MarkReadUrl { get; set; } = "/notifications/dropdown-handlers?handler=MarkRead";

    public string MarkAllReadUrl { get; set; } = "/notifications/dropdown-handlers?handler=MarkAllRead";

    public string ViewAllUrl { get; set; } = "/Notifications";

    public string RootId => Variant == "employer" ? "employerNotifDropdown" : "seekerNotifDropdown";

    public string TriggerClass => Variant == "employer"
        ? "employer-portal-topbar__icon-btn notif-dropdown__trigger"
        : "site-nav-icon-btn notif-dropdown__trigger";

    public string BadgeClass => Variant == "employer"
        ? "employer-portal-topbar__icon-badge notif-dropdown__badge"
        : "site-nav-icon-badge notif-dropdown__badge";

    public bool IsEmployer => string.Equals(Variant, "employer", StringComparison.OrdinalIgnoreCase);
}
