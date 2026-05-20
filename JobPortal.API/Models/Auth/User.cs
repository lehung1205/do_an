using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JobPortal.API.Models;

namespace JobPortal.API.Models.Auth;

[Table("users")]
public class User
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [MaxLength(255)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    [Column("phone_number")]
    public string? PhoneNumber { get; set; }

    [MaxLength(500)]
    [Column("profile_image")]
    public string? ProfileImage { get; set; }

    [Required]
    [MaxLength(32)]
    [Column("role")]
    public string Role { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    [Column("password_hash")]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(500)]
    [Column("password_reset_token_hash")]
    public string? PasswordResetTokenHash { get; set; }

    [Column("password_reset_token_expires_at")]
    public DateTime? PasswordResetTokenExpiresAt { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    public Admin? AdminProfile { get; set; }
    public Employer? EmployerProfile { get; set; }
    public JobSeeker? JobSeekerProfile { get; set; }
}
