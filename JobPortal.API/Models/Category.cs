using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobPortal.API.Models;

[Table("categories")]
public class Category
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("name")]
    [MaxLength(255)]
    public string Name { get; set; } = null!;

    public ICollection<Job> Jobs { get; set; } = new List<Job>();
}
