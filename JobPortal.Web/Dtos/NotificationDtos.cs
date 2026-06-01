namespace JobPortal.Web.Dtos;

public class NotificationUnreadSummaryDto
{
    public int UnreadCount { get; set; }
}

public class UserNotificationDto
{
    public long Id { get; set; }
    public string Type { get; set; } = "";
    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
    public string? ReferenceType { get; set; }
    public long? ReferenceId { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
