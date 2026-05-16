using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobPortal.API.Models;

[Table("applications")]
public class Application
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("job_seeker_id")]
    public long JobSeekerId { get; set; }

    [ForeignKey(nameof(JobSeekerId))]
    public JobSeeker JobSeeker { get; set; } = null!;

    [Column("job_id")]
    public long JobId { get; set; }

    [ForeignKey(nameof(JobId))]
    public Job Job { get; set; } = null!;

    [Column("resume_id")]
    public long ResumeId { get; set; }

    [ForeignKey(nameof(ResumeId))]
    public Resume Resume { get; set; } = null!;

    [Column("applied_at")]
    public DateTime AppliedAt { get; set; }

    [Column("status")]
    [MaxLength(32)]
    public string Status { get; set; } = null!;

    public ICollection<WorkExperience> WorkExperiences { get; set; } = new List<WorkExperience>();
}
