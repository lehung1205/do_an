using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobPortal.API.Models;

[Table("lich_su_thanh_toan")]
public class LichSuThanhToan
{
    [Key]
    [Column("id_thanh_toan")]
    public long IdThanhToan { get; set; }

    [Column("id_tuyen_dung")]
    public long IdTuyenDung { get; set; }

    [ForeignKey(nameof(IdTuyenDung))]
    public NhaTuyenDung NhaTuyenDung { get; set; } = null!;

    [Column("id_goi")]
    public long IdGoi { get; set; }

    [ForeignKey(nameof(IdGoi))]
    public GoiBaiDang GoiBaiDang { get; set; } = null!;

    [Column("gia")]
    public int Gia { get; set; }

    [Column("id_don_hang")]
    [MaxLength(100)]
    public string IdDonHang { get; set; } = null!;

    [Column("trang_thai")]
    [MaxLength(32)]
    public string TrangThai { get; set; } = null!;

    [Column("ngay_thanh_toan")]
    public DateTime? NgayThanhToan { get; set; }

    [Column("ngan_hang_thanh_toan")]
    [MaxLength(255)]
    public string? NganHangThanhToan { get; set; }

    [Column("ma_giao_dich")]
    [MaxLength(100)]
    public string? MaGiaoDich { get; set; }
}
