using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobPortal.API.Models;

[Table("nha_tuyen_dung")]
public class NhaTuyenDung
{
    [Key]
    [Column("id_tuyen_dung")]
    public long IdTuyenDung { get; set; }

    [Column("ten")]
    [MaxLength(255)]
    public string Ten { get; set; } = null!;

    [Column("email")]
    [MaxLength(255)]
    public string Email { get; set; } = null!;

    [Column("email_xac_thuc")]
    public DateTime? EmailXacThuc { get; set; }

    [Column("mat_khau")]
    [MaxLength(500)]
    public string MatKhau { get; set; } = null!;

    [Column("ngay_sinh")]
    public DateOnly? NgaySinh { get; set; }

    [Column("gioi_tinh")]
    public byte? GioiTinh { get; set; }

    [Column("mo_ta", TypeName = "text")]
    public string? MoTa { get; set; }

    [Column("anh")]
    [MaxLength(500)]
    public string? Anh { get; set; }

    [Column("so_luot_bai_dang")]
    public int SoLuotBaiDang { get; set; }

    [Column("cccd")]
    [MaxLength(20)]
    public string? Cccd { get; set; }

    [Column("sdt")]
    [MaxLength(20)]
    public string? Sdt { get; set; }

    [Column("trang_thai")]
    [MaxLength(32)]
    public string TrangThai { get; set; } = null!;

    public ICollection<CongViec> CongViecs { get; set; } = new List<CongViec>();
    public ICollection<DanhGia> DanhGias { get; set; } = new List<DanhGia>();
    public ICollection<LichSuThanhToan> LichSuThanhToans { get; set; } = new List<LichSuThanhToan>();
}
