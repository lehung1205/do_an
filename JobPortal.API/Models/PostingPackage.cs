using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobPortal.API.Models;

[Table("posting_packages")]
public class PostingPackage
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("admin_id")]
    public long AdminId { get; set; }

    [ForeignKey(nameof(AdminId))]
    public Admin Admin { get; set; } = null!;

    [Column("name")]
    [MaxLength(255)]
    public string Name { get; set; } = null!;

    [Column("description", TypeName = "text")]
    public string? Description { get; set; }

    [Column("price")]
    public int Price { get; set; }

    [Column("posting_limit")]
    public int PostingLimit { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    public ICollection<PaymentHistory> PaymentHistories { get; set; } = new List<PaymentHistory>();
}
