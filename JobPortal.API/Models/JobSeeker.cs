using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JobPortal.API.Models.Auth;

namespace JobPortal.API.Models;

[Table("job_seekers")]
public class JobSeeker
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

    [Column("profile_image")]
    [MaxLength(500)]
    public string? ProfileImage { get; set; }

    [Column("id_card")]
    [MaxLength(20)]
    public string? IdCard { get; set; }

    [Column("id_card_issue_date")]
    [MaxLength(50)]
    public string? IdCardIssueDate { get; set; }

    [Column("id_card_issue_place")]
    [MaxLength(255)]
    public string? IdCardIssuePlace { get; set; }

    [Column("permanent_address")]
    [MaxLength(500)]
    public string? PermanentAddress { get; set; }

    [Column("temporary_address")]
    [MaxLength(500)]
    public string? TemporaryAddress { get; set; }

    [Column("account_number")]
    [MaxLength(50)]
    public string? AccountNumber { get; set; }

    [Column("bank_name")]
    [MaxLength(255)]
    public string? BankName { get; set; }

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

    public ICollection<Resume> Resumes { get; set; } = new List<Resume>();
    public ICollection<Application> Applications { get; set; } = new List<Application>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
