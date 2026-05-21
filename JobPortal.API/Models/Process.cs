using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobPortal.API.Models;

[Table("process")]
public class Process
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("application_id")]
    public long ApplicationId { get; set; }

    [ForeignKey(nameof(ApplicationId))]
    public Application Application { get; set; } = null!;

    [Column("status")]
    [MaxLength(32)]
    public string Status { get; set; } = null!;

    [Column("title")]
    [MaxLength(255)]
    public string Title { get; set; } = null!;

    [Column("notes")]
    [MaxLength(2000)]
    public string? Notes { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}
