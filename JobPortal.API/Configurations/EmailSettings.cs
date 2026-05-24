namespace JobPortal.API.Configurations;

public class EmailSettings
{
    public bool Enabled { get; set; } = true;

    public string Host { get; set; } = "smtp.gmail.com";

    public int Port { get; set; } = 587;

    public bool UseSsl { get; set; } = true;

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string FromEmail { get; set; } = string.Empty;

    public string FromName { get; set; } = "JobPortal";

    /// <summary>Gửi SMTP thật khi bật và đã có username + app password.</summary>
    public bool IsSmtpConfigured =>
        Enabled
        && !string.IsNullOrWhiteSpace(Username)
        && !string.IsNullOrWhiteSpace(Password);
}
