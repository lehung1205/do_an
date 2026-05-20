namespace JobPortal.API.DTOs;

public class UpdateEmployerApplicationStatusRequest
{
    /// <summary>reviewed | accepted | rejected</summary>
    public string Status { get; set; } = null!;
}
