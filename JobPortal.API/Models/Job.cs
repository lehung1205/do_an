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
    public int Salary { get; set; }

    [Column("location")]
    [MaxLength(255)]
    public string Location { get; set; } = null!;

    [Column("posting_status")]
    [MaxLength(32)]
    public string PostingStatus { get; set; } = null!;

    [Column("start_date")]
    public DateTime StartDate { get; set; }

    [Column("end_date")]
    public DateTime EndDate { get; set; }

    [Column("expiry_date")]
    public DateTime ExpiryDate { get; set; }

    public ICollection<Image> Images { get; set; } = new List<Image>();
    public ICollection<Application> Applications { get; set; } = new List<Application>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
