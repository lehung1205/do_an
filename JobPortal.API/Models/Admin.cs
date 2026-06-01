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

    [Column("account_number")]
    [MaxLength(50)]
    public string? AccountNumber { get; set; }

    [Column("bank_name")]
    [MaxLength(255)]
    public string? BankName { get; set; }

    [Column("status")]
    [MaxLength(32)]
    public string Status { get; set; } = "ACTIVE";

    [Column("user_id")]
    public long UserId { get; set; }

    public User User { get; set; } = null!;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    public ICollection<PostingPackage> PostingPackages { get; set; } = new List<PostingPackage>();
}
