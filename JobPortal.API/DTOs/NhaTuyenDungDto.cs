namespace JobPortal.API.DTOs;

public class NhaTuyenDungDto
{
    public long id_tuyen_dung { get; set; }
    public string ten { get; set; } = null!;
    public string email { get; set; } = null!;
    public DateTime? email_xac_thuc { get; set; }
    public string mat_khau { get; set; } = null!;
    public DateOnly? ngay_sinh { get; set; }
    public byte? gioi_tinh { get; set; }
    public string? mo_ta { get; set; }
    public string? anh { get; set; }
    public int so_luot_bai_dang { get; set; }
    public string? cccd { get; set; }
    public string? sdt { get; set; }
    public string trang_thai { get; set; } = null!;
}
