using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JobPortal.API.Models.Auth;

namespace JobPortal.API.Models;

[Table("employers")]
public class Employer
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("name")]
    [MaxLength(255)]
    public string Name { get; set; } = null!;

    [Column("email_verified_at")]
    public DateTime? EmailVerifiedAt { get; set; }

    [Column("date_of_birth")]
    public DateOnly? DateOfBirth { get; set; }

    [Column("gender")]
    public byte? Gender { get; set; }

    [Column("description", TypeName = "text")]
    public string? Description { get; set; }

    [Column("image")]
    [MaxLength(500)]
    public string? Image { get; set; }

    [Column("posting_limit")]
    public int PostingLimit { get; set; }

    [Column("id_card")]
    [MaxLength(20)]
    public string? IdCard { get; set; }

    [Column("status")]
    [MaxLength(32)]
    public string Status { get; set; } = null!;

    [Column("user_id")]
    public long UserId { get; set; }

    public User User { get; set; } = null!;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Job> Jobs { get; set; } = new List<Job>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<PaymentHistory> PaymentHistories { get; set; } = new List<PaymentHistory>();
}
