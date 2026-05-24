using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobPortal.API.Models;

[Table("pending_registrations")]
public class PendingRegistration
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("registration_token")]
    [MaxLength(64)]
    public string RegistrationToken { get; set; } = null!;

    [Column("email")]
    [MaxLength(255)]
    public string Email { get; set; } = null!;

    [Column("name")]
    [MaxLength(255)]
    public string Name { get; set; } = null!;

    [Column("phone_number")]
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [Column("role")]
    [MaxLength(32)]
    public string Role { get; set; } = null!;

    [Column("password_hash")]
    [MaxLength(500)]
    public string PasswordHash { get; set; } = null!;

    [Column("otp_hash")]
    [MaxLength(500)]
    public string OtpHash { get; set; } = null!;

    [Column("otp_expires_at")]
    public DateTime OtpExpiresAt { get; set; }

    [Column("failed_attempts")]
    public int FailedAttempts { get; set; }

    [Column("last_otp_sent_at")]
    public DateTime LastOtpSentAt { get; set; }

    [Column("expires_at")]
    public DateTime ExpiresAt { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
