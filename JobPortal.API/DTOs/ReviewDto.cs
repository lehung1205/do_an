namespace JobPortal.API.DTOs;

public class ReviewDto
{
    public long Id { get; set; }
    public long JobId { get; set; }
    public long EmployerId { get; set; }
    public long JobSeekerId { get; set; }
    public string? Comment { get; set; }
    public int Rating { get; set; }
    public string ReviewType { get; set; } = null!;
}
