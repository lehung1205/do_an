namespace JobPortal.API.DTOs.Auth;

public class UpdateProfileRequest
{
    public string Name { get; set; } = null!;
    public string? PhoneNumber { get; set; }
}
