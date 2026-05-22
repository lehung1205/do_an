namespace JobPortal.Web.Dtos;

public class CreateJobRequest
{
    public long EmployerId { get; set; }
    public long CategoryId { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Salary { get; set; } = null!;
    public string Location { get; set; } = null!;
    public string PostingStatus { get; set; } = "recruiting";
    public string? WorkingHours { get; set; }
    public DateTime ExpiryDate { get; set; }
}
