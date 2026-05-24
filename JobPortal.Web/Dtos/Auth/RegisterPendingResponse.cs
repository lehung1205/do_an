namespace JobPortal.Web.Dtos.Auth;

public class RegisterPendingResponse
{
    public string RegistrationToken { get; set; } = null!;
    public string Email { get; set; } = null!;
    public DateTime OtpExpiresAt { get; set; }
    public string Message { get; set; } = null!;

    public string? DevOtp { get; set; }
}
