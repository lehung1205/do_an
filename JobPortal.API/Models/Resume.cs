using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobPortal.API.Models;

[Table("resumes")]
public class Resume
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("job_seeker_id")]
    public long JobSeekerId { get; set; }

    [ForeignKey(nameof(JobSeekerId))]
    public JobSeeker JobSeeker { get; set; } = null!;

    [Column("title")]
    [MaxLength(255)]
    public string Title { get; set; } = null!;

    [Column("url")]
    [MaxLength(500)]
    public string Url { get; set; } = null!;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    public ICollection<Application> Applications { get; set; } = new List<Application>();
}
