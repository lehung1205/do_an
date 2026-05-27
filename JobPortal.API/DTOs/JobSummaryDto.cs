namespace JobPortal.API.DTOs;

/// <summary>Minimal job payload for related-job sidebars.</summary>
public class JobSummaryDto
{
    public long Id { get; set; }
    public string Title { get; set; } = null!;
    public string EmployerName { get; set; } = null!;
    public string Location { get; set; } = null!;
    public string Salary { get; set; } = null!;
    public string? ThumbnailUrl { get; set; }
}
