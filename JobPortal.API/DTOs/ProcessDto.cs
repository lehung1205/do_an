namespace JobPortal.API.DTOs;

public class ProcessDto
{
    public long Id { get; set; }
    public long ApplicationId { get; set; }
    public string Status { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
