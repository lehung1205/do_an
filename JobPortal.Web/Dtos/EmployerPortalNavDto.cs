namespace JobPortal.Web.Dtos;

public class EmployerPortalNavDto
{
    public string CompanyName { get; set; } = "";
    public int PostingLimit { get; set; }
    public int TotalJobs { get; set; }
    public int UnreadApplications { get; set; }
    public int UnreadMessages { get; set; }
}
