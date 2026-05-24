namespace JobPortal.Web.Dtos;

public class ChatThreadDto
{
    public long ApplicationId { get; set; }
    public long PartnerUserId { get; set; }
    public string Title { get; set; } = null!;
    public string Subtitle { get; set; } = null!;
    public bool PartnerIsOnline { get; set; }
    public int UnreadCount { get; set; }
    public string? LastMessagePreview { get; set; }
    public DateTime? LastMessageAt { get; set; }
}

public class ChatMessageDto
{
    public long Id { get; set; }
    public long ApplicationId { get; set; }
    public long SenderUserId { get; set; }
    public string SenderName { get; set; } = null!;
    public string Content { get; set; } = null!;
    public DateTime SentAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public bool IsMine { get; set; }
}

public class ChatJoinedDto
{
    public long ApplicationId { get; set; }
    public long PartnerUserId { get; set; }
    public string PartnerName { get; set; } = null!;
    public string JobTitle { get; set; } = null!;
    public bool PartnerIsOnline { get; set; }
}

public class ChatUnreadSummaryDto
{
    public int TotalUnreadCount { get; set; }
}
