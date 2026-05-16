using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JobPortal.API.Models.Auth;

namespace JobPortal.API.Models;

[Table("admins")]
public class Admin
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("name")]
    [MaxLength(255)]
    public string Name { get; set; } = null!;

    [Column("email")]
    [MaxLength(255)]
    public string Email { get; set; } = null!;

    [Column("password_hash")]
    [MaxLength(500)]
    public string PasswordHash { get; set; } = null!;

    [Column("account_number")]
    [MaxLength(50)]
    public string? AccountNumber { get; set; }

    [Column("bank_name")]
    [MaxLength(255)]
    public string? BankName { get; set; }

    [Column("phone")]
    [MaxLength(20)]
    public string? Phone { get; set; }

    [Column("status")]
    [MaxLength(32)]
    public string Status { get; set; } = "ACTIVE";

    [Column("role")]
    [MaxLength(32)]
    public string Role { get; set; } = "ADMIN";

    [Column("user_id")]
    public long UserId { get; set; }

    public User? User { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    public ICollection<PostingPackage> PostingPackages { get; set; } = new List<PostingPackage>();
}
