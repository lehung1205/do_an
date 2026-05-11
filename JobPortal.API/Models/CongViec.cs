using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobPortal.API.Models;

[Table("cong_viec")]
public class CongViec
{
    [Key]
    [Column("id_cong_viec")]
    public long IdCongViec { get; set; }

    [Column("id_tuyen_dung")]
    public long IdTuyenDung { get; set; }

    [ForeignKey(nameof(IdTuyenDung))]
    public NhaTuyenDung NhaTuyenDung { get; set; } = null!;

    [Column("id_danh_muc")]
    public long IdDanhMuc { get; set; }

    [ForeignKey(nameof(IdDanhMuc))]
    public DanhMuc DanhMuc { get; set; } = null!;

    [Column("tieu_de")]
    [MaxLength(500)]
    public string TieuDe { get; set; } = null!;

    [Column("mo_ta", TypeName = "text")]
    public string MoTa { get; set; } = null!;

    [Column("muc_luong")]
    public int MucLuong { get; set; }

    [Column("dia_diem")]
    [MaxLength(255)]
    public string DiaDiem { get; set; } = null!;

    [Column("trang_thai_bai_dang")]
    [MaxLength(32)]
    public string TrangThaiBaiDang { get; set; } = null!;

    [Column("ngay_bat_dau")]
    public DateTime NgayBatDau { get; set; }

    [Column("ngay_ket_thuc")]
    public DateTime NgayKetThuc { get; set; }

    [Column("ngay_het_han")]
    public DateTime NgayHetHan { get; set; }

    public ICollection<HinhAnh> HinhAnhs { get; set; } = new List<HinhAnh>();
    public ICollection<UngTuyen> UngTuyens { get; set; } = new List<UngTuyen>();
    public ICollection<DanhGia> DanhGias { get; set; } = new List<DanhGia>();
}
