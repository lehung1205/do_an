using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobPortal.API.Models;

[Table("reviews")]
public class Review
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("job_id")]
    public long JobId { get; set; }

    [ForeignKey(nameof(JobId))]
    public Job Job { get; set; } = null!;

    [Column("employer_id")]
    public long EmployerId { get; set; }

    [ForeignKey(nameof(EmployerId))]
    public Employer Employer { get; set; } = null!;

    [Column("job_seeker_id")]
    public long JobSeekerId { get; set; }

    [ForeignKey(nameof(JobSeekerId))]
    public JobSeeker JobSeeker { get; set; } = null!;

    [Column("comment", TypeName = "text")]
    public string? Comment { get; set; }

    [Column("rating")]
    public int Rating { get; set; }

    [Column("review_type")]
    [MaxLength(32)]
    public string ReviewType { get; set; } = null!;
}
