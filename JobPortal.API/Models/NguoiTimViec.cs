using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobPortal.API.Models;

[Table("nguoi_tim_viec")]
public class NguoiTimViec
{
    [Key]
    [Column("id_tim_viec")]
    public long IdTimViec { get; set; }

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

    [Column("sdt")]
    [MaxLength(20)]
    public string? Sdt { get; set; }

    [Column("ngay_sinh")]
    public DateOnly? NgaySinh { get; set; }

    [Column("gioi_tinh")]
    public byte? GioiTinh { get; set; }

    [Column("mo_ta", TypeName = "text")]
    public string? MoTa { get; set; }

    [Column("anh_dai_dien")]
    [MaxLength(500)]
    public string? AnhDaiDien { get; set; }

    [Column("cccd")]
    [MaxLength(20)]
    public string? Cccd { get; set; }

    [Column("ngay_cap_cccd")]
    [MaxLength(50)]
    public string? NgayCapCccd { get; set; }

    [Column("noi_cap")]
    [MaxLength(255)]
    public string? NoiCap { get; set; }

    [Column("dia_chi_thuong_tru")]
    [MaxLength(500)]
    public string? DiaChiThuongTru { get; set; }

    [Column("dia_chi_tam_tru")]
    [MaxLength(500)]
    public string? DiaChiTamTru { get; set; }

    [Column("so_tai_khoan")]
    [MaxLength(50)]
    public string? SoTaiKhoan { get; set; }

    [Column("ten_ngan_hang")]
    [MaxLength(255)]
    public string? TenNganHang { get; set; }

    [Column("trang_thai")]
    [MaxLength(32)]
    public string TrangThai { get; set; } = null!;

    public ICollection<HoSoTuyenDung> HoSoTuyenDungs { get; set; } = new List<HoSoTuyenDung>();
    public ICollection<UngTuyen> UngTuyens { get; set; } = new List<UngTuyen>();
    public ICollection<DanhGia> DanhGias { get; set; } = new List<DanhGia>();
}
