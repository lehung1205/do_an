namespace JobPortal.Web.Dtos;

public class CreateApplicationRequest
{
    public long JobId { get; set; }
    public long ResumeId { get; set; }
}

public class MyApplicationDto
{
    public long Id { get; set; }
    public long JobId { get; set; }
    public string JobTitle { get; set; } = null!;
    public string EmployerName { get; set; } = null!;
    public string CategoryName { get; set; } = null!;
    public string JobLocation { get; set; } = null!;
    public string JobSalary { get; set; } = null!;
    public string? JobWorkingHours { get; set; }
    public string JobPostingStatus { get; set; } = null!;
    public DateTime JobExpiryDate { get; set; }
    public string? JobThumbnailUrl { get; set; }
    public long ResumeId { get; set; }
    public string ResumeTitle { get; set; } = null!;
    public string? ResumeUrl { get; set; }
    public DateTime AppliedAt { get; set; }
    public string Status { get; set; } = null!;
}
