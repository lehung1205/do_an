namespace JobPortal.Web.Dtos.Auth;

public class ResendRegisterOtpRequest
{
    public string RegistrationToken { get; set; } = null!;
}
