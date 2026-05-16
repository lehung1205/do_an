using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobPortal.API.Models.Auth;

[Table("refresh_tokens")]
public class RefreshToken
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("user_id")]
    public long UserId { get; set; }

    [Column("token_hash")]
    public string TokenHash { get; set; } = string.Empty;

    [Column("expires_at")]
    public DateTime ExpiresAt { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("created_by_ip")]
    public string CreatedByIp { get; set; } = string.Empty;

    [Column("revoked_at")]
    public DateTime? RevokedAt { get; set; }

    [Column("revoked_by_ip")]
    public string? RevokedByIp { get; set; }

    [Column("replaced_by_token")]
    public string? ReplacedByToken { get; set; }

    [Column("replaced_at")]
    public DateTime? ReplacedAt { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt != null;
    public bool IsUsed => ReplacedAt != null;
    public bool IsActive => !IsRevoked && !IsExpired && !IsUsed;

    public User? User { get; set; }
}
