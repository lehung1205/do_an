namespace JobPortal.API.DTOs;

public class ApplicationDto
{
    public long Id { get; set; }
    public long JobSeekerId { get; set; }
    public long JobId { get; set; }
    public long ResumeId { get; set; }
    public DateTime AppliedAt { get; set; }
    public string Status { get; set; } = null!;
}
