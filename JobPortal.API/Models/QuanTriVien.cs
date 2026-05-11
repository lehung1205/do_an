using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobPortal.API.Models;

[Table("quan_tri_vien")]
public class QuanTriVien
{
    [Key]
    [Column("id_quan_tri")]
    public long IdQuanTri { get; set; }

    [Column("ten")]
    [MaxLength(255)]
    public string Ten { get; set; } = null!;

    [Column("email")]
    [MaxLength(255)]
    public string Email { get; set; } = null!;

    [Column("mat_khau")]
    [MaxLength(500)]
    public string MatKhau { get; set; } = null!;

    [Column("so_tai_khoan")]
    [MaxLength(50)]
    public string? SoTaiKhoan { get; set; }

    [Column("ten_ngan_hang")]
    [MaxLength(255)]
    public string? TenNganHang { get; set; }

    public ICollection<GoiBaiDang> GoiBaiDangs { get; set; } = new List<GoiBaiDang>();
}
