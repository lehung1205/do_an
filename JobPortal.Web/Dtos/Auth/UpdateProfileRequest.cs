namespace JobPortal.Web.Dtos.Auth;

public class UpdateProfileRequest
{
    public string Name { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string? ProfileImage { get; set; }
}
