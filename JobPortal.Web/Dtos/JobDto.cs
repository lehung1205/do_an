namespace JobPortal.Web.Dtos;

public class JobDto
{
    public long Id { get; set; }
    public long EmployerId { get; set; }
    public string EmployerName { get; set; } = null!;
    public double? EmployerAverageRating { get; set; }
    public int EmployerReviewCount { get; set; }
    public long CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Salary { get; set; } = null!;
    public string Location { get; set; } = null!;
    public string PostingStatus { get; set; } = null!;
    public string? WorkingHours { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string? ThumbnailUrl { get; set; }
}
