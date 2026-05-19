namespace JobPortal.Web.Dtos.Auth;

public class LoginRequest
{
    public string Identifier { get; set; } = null!;
    public string Password { get; set; } = null!;
}
