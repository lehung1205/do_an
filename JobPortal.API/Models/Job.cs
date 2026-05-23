using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobPortal.API.Models;

[Table("jobs")]
public class Job
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("employer_id")]
    public long EmployerId { get; set; }

    [ForeignKey(nameof(EmployerId))]
    public Employer Employer { get; set; } = null!;

    [Column("category_id")]
    public long CategoryId { get; set; }

    [ForeignKey(nameof(CategoryId))]
    public Category Category { get; set; } = null!;

    [Column("title")]
    [MaxLength(500)]
    public string Title { get; set; } = null!;

    [Column("description", TypeName = "text")]
    public string Description { get; set; } = null!;

    [Column("salary")]
    [MaxLength(255)]
    public string Salary { get; set; } = null!;

    [Column("location")]
    [MaxLength(255)]
    public string Location { get; set; } = null!;

    [Column("posting_status")]
    [MaxLength(32)]
    public string PostingStatus { get; set; } = null!;

    [Column("working_hours")]
    [MaxLength(50)]
    public string? WorkingHours { get; set; }

    [Column("expiry_date")]
    public DateTime ExpiryDate { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Image> Images { get; set; } = new List<Image>();
    public ICollection<Application> Applications { get; set; } = new List<Application>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
