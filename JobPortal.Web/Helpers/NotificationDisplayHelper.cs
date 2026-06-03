using JobPortal.Web.Dtos;

namespace JobPortal.Web.Helpers;

public static class NotificationDisplayHelper
{
    public const string GroupNew = "Mới";
    public const string GroupToday = "Hôm nay";
    public const string GroupEarlier = "Trước đó";

    public static string IconClass(string type) => type switch
    {
        "job_approved" or "application_accepted" => "bi-check-circle-fill text-success",
        "job_rejected" or "application_rejected" => "bi-x-circle-fill text-danger",
        "work_progress_updated" => "bi-graph-up-arrow text-primary",
        "new_application" => "bi-person-plus-fill text-primary",
        _ => "bi-bell-fill text-primary"
    };

    public static string FormatRelativeTime(DateTime createdAtUtc)
    {
        var created = createdAtUtc.ToLocalTime();
        var diff = DateTime.Now - created;

        if (diff.TotalMinutes < 1)
        {
            return "Vừa xong";
        }

        if (diff.TotalHours < 1)
        {
            return $"{(int)diff.TotalMinutes} phút trước";
        }

        if (diff.TotalDays < 1)
        {
            return $"{(int)diff.TotalHours} giờ trước";
        }

        if (diff.TotalDays < 7)
        {
            return $"{(int)diff.TotalDays} ngày trước";
        }

        return created.ToString("dd/MM/yyyy HH:mm");
    }

    public static string FormatShortTime(DateTime createdAtUtc)
    {
        var created = createdAtUtc.ToLocalTime();
        var diff = DateTime.Now - created;

        if (diff.TotalMinutes < 1)
        {
            return "Vừa xong";
        }

        if (diff.TotalHours < 1)
        {
            return $"{(int)diff.TotalMinutes} phút";
        }

        if (diff.TotalDays < 1)
        {
            return $"{(int)diff.TotalHours} giờ";
        }

        if (diff.TotalDays < 7)
        {
            return $"{(int)diff.TotalDays} ngày";
        }

        return created.ToString("dd/MM/yyyy");
    }

    public static string GetTimeGroup(DateTime createdAtUtc)
    {
        var local = createdAtUtc.ToLocalTime();
        var now = DateTime.Now;

        if ((now - local).TotalHours < 2)
        {
            return GroupNew;
        }

        if (local.Date == now.Date)
        {
            return GroupToday;
        }

        return GroupEarlier;
    }

    public static string? GetSeekerActionUrl(UserNotificationDto notification, Func<string, object?, string?> urlPage)
    {
        if (notification.ReferenceId == null || string.IsNullOrWhiteSpace(notification.ReferenceType))
        {
            return null;
        }

        if (!string.Equals(notification.ReferenceType, "application", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (string.Equals(notification.Type, "work_progress_updated", StringComparison.OrdinalIgnoreCase))
        {
            return urlPage("/Applications/WorkProgress/Detail", new { applicationId = notification.ReferenceId });
        }

        var status = string.Equals(notification.Type, "application_accepted", StringComparison.OrdinalIgnoreCase)
            ? "accepted"
            : "rejected";

        return urlPage("/Applications/Index", new { status });
    }

    public static string? GetEmployerActionUrl(UserNotificationDto notification, Func<string, object?, string?> urlPage)
    {
        if (notification.ReferenceId == null || string.IsNullOrWhiteSpace(notification.ReferenceType))
        {
            return null;
        }

        if (string.Equals(notification.ReferenceType, "job", StringComparison.OrdinalIgnoreCase))
        {
            return urlPage("/Jobs/Detail", new { id = notification.ReferenceId.Value });
        }

        if (string.Equals(notification.ReferenceType, "application", StringComparison.OrdinalIgnoreCase)
            && string.Equals(notification.Type, "new_application", StringComparison.OrdinalIgnoreCase))
        {
            return urlPage("/Employer/Applicants", new { q = notification.Title });
        }

        return null;
    }

    public static IReadOnlyList<NotificationGroupModel> GroupNotifications(
        IEnumerable<UserNotificationDto> notifications,
        Func<UserNotificationDto, string?> resolveActionUrl)
    {
        var order = new[] { GroupNew, GroupToday, GroupEarlier };
        var grouped = notifications
            .GroupBy(n => GetTimeGroup(n.CreatedAt))
            .ToDictionary(g => g.Key, g => g.OrderByDescending(n => n.CreatedAt).ThenByDescending(n => n.Id).ToList());

        var result = new List<NotificationGroupModel>();
        foreach (var label in order)
        {
            if (!grouped.TryGetValue(label, out var items) || items.Count == 0)
            {
                continue;
            }

            result.Add(new NotificationGroupModel
            {
                Label = label,
                Items = items.Select(n => new NotificationItemModel
                {
                    Id = n.Id,
                    Type = n.Type,
                    Title = n.Title,
                    Message = n.Message,
                    IsRead = n.IsRead,
                    TimeLabel = FormatShortTime(n.CreatedAt),
                    IconClass = IconClass(n.Type),
                    ActionUrl = resolveActionUrl(n)
                }).ToList()
            });
        }

        return result;
    }

    public class NotificationGroupModel
    {
        public string Label { get; set; } = "";
        public IReadOnlyList<NotificationItemModel> Items { get; set; } = Array.Empty<NotificationItemModel>();
    }

    public class NotificationItemModel
    {
        public long Id { get; set; }
        public string Type { get; set; } = "";
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
        public bool IsRead { get; set; }
        public string TimeLabel { get; set; } = "";
        public string IconClass { get; set; } = "";
        public string? ActionUrl { get; set; }
    }
}
