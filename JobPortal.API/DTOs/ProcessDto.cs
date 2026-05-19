namespace JobPortal.API.DTOs;

public class ProcessDto
{
    public long Id { get; set; }
    public long ApplicationId { get; set; }
    public string Status { get; set; } = null!;
}
