namespace JobPortal.API.DTOs.Auth;

public class ResendRegisterOtpRequest
{
    public string RegistrationToken { get; set; } = null!;
}
