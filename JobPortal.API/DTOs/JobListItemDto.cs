namespace JobPortal.API.DTOs;

/// <summary>Lightweight job payload for public listing endpoints.</summary>
public class JobListItemDto
{
    public long Id { get; set; }
    public long CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Title { get; set; } = null!;
    public string DescriptionPreview { get; set; } = "";
    public string Salary { get; set; } = null!;
    public string Location { get; set; } = null!;
    public string? WorkingHours { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string EmployerName { get; set; } = null!;
    public double? EmployerAverageRating { get; set; }
    public int EmployerReviewCount { get; set; }
}
