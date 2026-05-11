using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobPortal.API.Models;

[Table("hinh_anh")]
public class HinhAnh
{
    [Key]
    [Column("id_hinh_anh")]
    public long IdHinhAnh { get; set; }

    [Column("id_cong_viec")]
    public long IdCongViec { get; set; }

    [ForeignKey(nameof(IdCongViec))]
    public CongViec CongViec { get; set; } = null!;

    [Column("duong_dan_url")]
    [MaxLength(500)]
    public string DuongDanUrl { get; set; } = null!;

    [Column("ten")]
    [MaxLength(255)]
    public string? Ten { get; set; }
}
