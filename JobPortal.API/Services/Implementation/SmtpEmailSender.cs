using JobPortal.API.Configurations;
using JobPortal.API.Exceptions;
using JobPortal.API.Services.Interface;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace JobPortal.API.Services.Implementation;

public class SmtpEmailSender : IEmailSender
{
    private readonly EmailSettings _settings;
    private readonly ILogger<SmtpEmailSender> _logger;
    private readonly IHostEnvironment _environment;

    public SmtpEmailSender(
        IOptions<EmailSettings> settings,
        ILogger<SmtpEmailSender> logger,
        IHostEnvironment environment)
    {
        _settings = settings.Value;
        _logger = logger;
        _environment = environment;
    }

    public async Task SendAsync(
        string toEmail,
        string subject,
        string htmlBody,
        string? otpCodeForLog = null,
        CancellationToken cancellationToken = default)
    {
        if (!_settings.IsSmtpConfigured)
        {
            LogDevOtpFallback(toEmail, subject, otpCodeForLog);
            return;
        }

        var fromEmail = string.IsNullOrWhiteSpace(_settings.FromEmail)
            ? _settings.Username.Trim()
            : _settings.FromEmail.Trim();

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, fromEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlBody };

        try
        {
            using var client = new SmtpClient();
            await client.ConnectAsync(
                _settings.Host,
                _settings.Port,
                _settings.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto,
                cancellationToken);

            await client.AuthenticateAsync(
                _settings.Username.Trim(),
                _settings.Password,
                cancellationToken);

            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation("Đã gửi email tới {Email} (tiêu đề: {Subject})", toEmail, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gửi email SMTP thất bại tới {Email}", toEmail);
            throw new BadRequestException(
                "Không gửi được email. Kiểm tra cấu hình Gmail (App Password) trong appsettings.Development.json hoặc User Secrets.");
        }
    }

    private void LogDevOtpFallback(string toEmail, string subject, string? otpCodeForLog)
    {
        if (!string.IsNullOrWhiteSpace(otpCodeForLog))
        {
            _logger.LogWarning(
                "SMTP chưa bật (Email.Enabled + Username + Password). MÃ OTP đăng ký: {OtpCode} → {Email}",
                otpCodeForLog,
                toEmail);
            return;
        }

        _logger.LogWarning(
            "SMTP chưa bật. Không gửi email tới {Email}, tiêu đề: {Subject}. " +
            "Bật Email trong appsettings.Development.json hoặc xem tài liệu Email:Gmail.",
            toEmail,
            subject);
    }
}
