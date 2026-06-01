namespace JobPortal.API.DTOs;

public class NotificationUnreadSummaryDto
{
    public int UnreadCount { get; set; }
}

public class UserNotificationDto
{
    public long Id { get; set; }
    public string Type { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Message { get; set; } = null!;
    public string? ReferenceType { get; set; }
    public long? ReferenceId { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
