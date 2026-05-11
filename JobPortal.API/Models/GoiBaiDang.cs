using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobPortal.API.Models;

[Table("goi_bai_dang")]
public class GoiBaiDang
{
    [Key]
    [Column("id_goi")]
    public long IdGoi { get; set; }

    [Column("id_quan_tri")]
    public long IdQuanTri { get; set; }

    [ForeignKey(nameof(IdQuanTri))]
    public QuanTriVien QuanTriVien { get; set; } = null!;

    [Column("ten")]
    [MaxLength(255)]
    public string Ten { get; set; } = null!;

    [Column("gia")]
    public int Gia { get; set; }

    [Column("so_luot_dang_bai")]
    public int SoLuotDangBai { get; set; }

    public ICollection<LichSuThanhToan> LichSuThanhToans { get; set; } = new List<LichSuThanhToan>();
}
