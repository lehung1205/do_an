namespace JobPortal.API.Models;

public class Job
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Company { get; set; } = null!;
    public string Location { get; set; } = null!;
    public decimal Salary { get; set; }
}