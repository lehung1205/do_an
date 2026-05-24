namespace JobPortal.API.DTOs;

public class AdminPendingJobDto
{
    public long Id { get; set; }
    public long EmployerId { get; set; }
    public string EmployerName { get; set; } = null!;
    public string? EmployerEmail { get; set; }
    public long CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Salary { get; set; } = null!;
    public string Location { get; set; } = null!;
    public string PostingStatus { get; set; } = null!;
    public string? WorkingHours { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string? ThumbnailUrl { get; set; }
}

public class RejectJobRequest
{
    public string? Reason { get; set; }
}
