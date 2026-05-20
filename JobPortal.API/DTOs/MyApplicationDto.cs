namespace JobPortal.API.DTOs;

public class MyApplicationDto
{
    public long Id { get; set; }
    public long JobId { get; set; }
    public string JobTitle { get; set; } = null!;
    public string JobLocation { get; set; } = null!;
    public int JobSalary { get; set; }
    public string JobPostingStatus { get; set; } = null!;
    public long ResumeId { get; set; }
    public string ResumeTitle { get; set; } = null!;
    public string? ResumeUrl { get; set; }
    public DateTime AppliedAt { get; set; }
    public string Status { get; set; } = null!;
}
