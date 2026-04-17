namespace JobPortal.API.DTOs;

public class ApplicationDto
{
    public int Id { get; set; }
    public int JobId { get; set; }
    public int UserId { get; set; }
    public DateTime AppliedDate { get; set; }
    public string Status { get; set; } = null!;
    
    public string? JobTitle { get; set; }
    public string? UserFullName { get; set; }
}
