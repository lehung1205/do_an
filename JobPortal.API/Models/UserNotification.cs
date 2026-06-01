using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JobPortal.API.Models.Auth;

namespace JobPortal.API.Models;

[Table("user_notifications")]
public class UserNotification
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("user_id")]
    public long UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    [Column("type")]
    [MaxLength(64)]
    public string Type { get; set; } = null!;

    [Column("title")]
    [MaxLength(255)]
    public string Title { get; set; } = null!;

    [Column("message", TypeName = "text")]
    public string Message { get; set; } = null!;

    [Column("reference_type")]
    [MaxLength(32)]
    public string? ReferenceType { get; set; }

    [Column("reference_id")]
    public long? ReferenceId { get; set; }

    [Column("is_read")]
    public bool IsRead { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
