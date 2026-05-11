using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobPortal.API.Models;

[Table("ung_tuyen")]
public class UngTuyen
{
    [Key]
    [Column("id_ung_tuyen")]
    public long IdUngTuyen { get; set; }

    [Column("id_tim_viec")]
    public long IdTimViec { get; set; }

    [ForeignKey(nameof(IdTimViec))]
    public NguoiTimViec NguoiTimViec { get; set; } = null!;

    [Column("id_cong_viec")]
    public long IdCongViec { get; set; }

    [ForeignKey(nameof(IdCongViec))]
    public CongViec CongViec { get; set; } = null!;

    [Column("id_cv")]
    public long IdCv { get; set; }

    [ForeignKey(nameof(IdCv))]
    public HoSoTuyenDung HoSoTuyenDung { get; set; } = null!;

    [Column("ngay_ung_tuyen")]
    public DateTime NgayUngTuyen { get; set; }

    [Column("trang_thai")]
    [MaxLength(32)]
    public string TrangThai { get; set; } = null!;

    public ICollection<QuaTrinhLamViec> QuaTrinhLamViecs { get; set; } = new List<QuaTrinhLamViec>();
}
