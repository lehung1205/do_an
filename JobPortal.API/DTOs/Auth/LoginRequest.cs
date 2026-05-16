namespace JobPortal.API.DTOs.Auth;

public class LoginRequest
{
    public string Identifier { get; set; } = null!; // email or phone
    public string Password { get; set; } = null!;
}
