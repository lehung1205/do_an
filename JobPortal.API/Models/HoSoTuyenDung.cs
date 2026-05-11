using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobPortal.API.Models;

[Table("ho_so_tuyen_dung")]
public class HoSoTuyenDung
{
    [Key]
    [Column("id_cv")]
    public long IdCv { get; set; }

    [Column("id_tim_viec")]
    public long IdTimViec { get; set; }

    [ForeignKey(nameof(IdTimViec))]
    public NguoiTimViec NguoiTimViec { get; set; } = null!;

    [Column("tieu_de")]
    [MaxLength(255)]
    public string TieuDe { get; set; } = null!;

    [Column("url")]
    [MaxLength(500)]
    public string Url { get; set; } = null!;

    [Column("ngay_tao")]
    public DateTime NgayTao { get; set; }

    public ICollection<UngTuyen> UngTuyens { get; set; } = new List<UngTuyen>();
}
