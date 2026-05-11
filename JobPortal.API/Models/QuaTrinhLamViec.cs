using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobPortal.API.Models;

[Table("qua_trinh_lam_viec")]
public class QuaTrinhLamViec
{
    [Key]
    [Column("id_lam_viec")]
    public long IdLamViec { get; set; }

    [Column("id_ung_tuyen")]
    public long IdUngTuyen { get; set; }

    [ForeignKey(nameof(IdUngTuyen))]
    public UngTuyen UngTuyen { get; set; } = null!;

    [Column("trang_thai")]
    [MaxLength(32)]
    public string TrangThai { get; set; } = null!;
}
