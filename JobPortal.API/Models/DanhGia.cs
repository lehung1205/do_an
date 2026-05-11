using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobPortal.API.Models;

[Table("danh_gia")]
public class DanhGia
{
    [Key]
    [Column("id_danh_gia")]
    public long IdDanhGia { get; set; }

    [Column("id_cong_viec")]
    public long IdCongViec { get; set; }

    [ForeignKey(nameof(IdCongViec))]
    public CongViec CongViec { get; set; } = null!;

    [Column("id_tuyen_dung")]
    public long IdTuyenDung { get; set; }

    [ForeignKey(nameof(IdTuyenDung))]
    public NhaTuyenDung NhaTuyenDung { get; set; } = null!;

    [Column("id_tim_viec")]
    public long IdTimViec { get; set; }

    [ForeignKey(nameof(IdTimViec))]
    public NguoiTimViec NguoiTimViec { get; set; } = null!;

    [Column("binh_luan", TypeName = "text")]
    public string? BinhLuan { get; set; }

    [Column("danh_gia")]
    public int Diem { get; set; }

    [Column("danh_gia_toi")]
    [MaxLength(32)]
    public string DanhGiaToi { get; set; } = null!;
}
