namespace JobPortal.Web.Dtos;

public class JobDto
{
    public long Id { get; set; }
    public long EmployerId { get; set; }
    public long CategoryId { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public int Salary { get; set; }
    public string Location { get; set; } = null!;
    public string PostingStatus { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime ExpiryDate { get; set; }
}
