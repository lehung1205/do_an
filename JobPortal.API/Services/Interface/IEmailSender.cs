namespace JobPortal.API.Services.Interface;

public interface IEmailSender
{
    /// <param name="otpCodeForLog">Mã OTP thuần — ghi log rõ khi SMTP chưa cấu hình (dev).</param>
    Task SendAsync(
        string toEmail,
        string subject,
        string htmlBody,
        string? otpCodeForLog = null,
        CancellationToken cancellationToken = default);
}
