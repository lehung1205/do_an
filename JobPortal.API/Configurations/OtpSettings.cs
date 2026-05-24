namespace JobPortal.API.Configurations;

public class OtpSettings
{
    public int CodeLength { get; set; } = 6;

    public int ExpirationMinutes { get; set; } = 10;

    public int PendingRegistrationMinutes { get; set; } = 30;

    public int MaxVerifyAttempts { get; set; } = 5;

    public int ResendCooldownSeconds { get; set; } = 60;
}
