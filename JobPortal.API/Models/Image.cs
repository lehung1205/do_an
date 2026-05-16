using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobPortal.API.Models;

[Table("images")]
public class Image
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("job_id")]
    public long JobId { get; set; }

    [ForeignKey(nameof(JobId))]
    public Job Job { get; set; } = null!;

    [Column("url")]
    [MaxLength(500)]
    public string Url { get; set; } = null!;

    [Column("name")]
    [MaxLength(255)]
    public string? Name { get; set; }
}
