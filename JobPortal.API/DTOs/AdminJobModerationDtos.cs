namespace JobPortal.API.DTOs;

public class AdminJobModerationSummaryDto
{
    public int PendingCount { get; set; }
    public int RecruitingCount { get; set; }
    public int RejectedCount { get; set; }
    public int ClosedCount { get; set; }
}

public class AdminPendingJobDto
{
    public long Id { get; set; }
    public long EmployerId { get; set; }
    public string EmployerName { get; set; } = null!;
    public string? EmployerEmail { get; set; }
    public string? EmployerPhone { get; set; }
    public string? EmployerImage { get; set; }
    public long CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string DescriptionPreview { get; set; } = null!;
    public string Salary { get; set; } = null!;
    public string Location { get; set; } = null!;
    public string PostingStatus { get; set; } = null!;
    public string? WorkingHours { get; set; }
    public int ApplicantCount { get; set; }
    public int ImageCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string? ThumbnailUrl { get; set; }
}

public class RejectJobRequest
{
    public string? Reason { get; set; }
}
