namespace JobPortal.API.Models;

public class JobApplication
{
    public int Id { get; set; }
    public int JobId { get; set; }
    public int UserId { get; set; }
    public DateTime AppliedDate { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Pending";
    
    public Job Job { get; set; } = null!;
    public User User { get; set; } = null!;
}
