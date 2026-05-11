using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobPortal.API.Models;

[Table("danh_muc")]
public class DanhMuc
{
    [Key]
    [Column("id_danh_muc")]
    public long IdDanhMuc { get; set; }

    [Column("ten")]
    [MaxLength(255)]
    public string Ten { get; set; } = null!;

    public ICollection<CongViec> CongViecs { get; set; } = new List<CongViec>();
}
