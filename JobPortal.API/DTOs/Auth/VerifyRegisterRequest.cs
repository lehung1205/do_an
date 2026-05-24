namespace JobPortal.API.DTOs.Auth;

public class VerifyRegisterRequest
{
    public string RegistrationToken { get; set; } = null!;
    public string OtpCode { get; set; } = null!;
}
