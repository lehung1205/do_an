namespace JobPortal.API.DTOs.Auth;

public class RegisterPendingResponse
{
    public string RegistrationToken { get; set; } = null!;
    public string Email { get; set; } = null!;
    public DateTime OtpExpiresAt { get; set; }
    public string Message { get; set; } = null!;

    /// <summary>Chỉ trả về khi Development và chưa cấu hình SMTP.</summary>
    public string? DevOtp { get; set; }
}
