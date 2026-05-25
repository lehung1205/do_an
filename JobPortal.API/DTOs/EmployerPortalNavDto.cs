namespace JobPortal.API.DTOs;

public class EmployerPortalNavDto
{
    public string CompanyName { get; set; } = null!;
    public int PostingLimit { get; set; }
    public int TotalJobs { get; set; }
    public int UnreadApplications { get; set; }
    public int UnreadMessages { get; set; }
}
