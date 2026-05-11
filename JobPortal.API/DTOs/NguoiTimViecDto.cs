namespace JobPortal.API.DTOs;

public class NguoiTimViecDto
{
    public long id_tim_viec { get; set; }
    public string ten { get; set; } = null!;
    public string email { get; set; } = null!;
    public DateTime? email_xac_thuc { get; set; }
    public string mat_khau { get; set; } = null!;
    public string? sdt { get; set; }
    public DateOnly? ngay_sinh { get; set; }
    public byte? gioi_tinh { get; set; }
    public string? mo_ta { get; set; }
    public string? anh_dai_dien { get; set; }
    public string? cccd { get; set; }
    public string? ngay_cap_cccd { get; set; }
    public string? noi_cap { get; set; }
    public string? dia_chi_thuong_tru { get; set; }
    public string? dia_chi_tam_tru { get; set; }
    public string? so_tai_khoan { get; set; }
    public string? ten_ngan_hang { get; set; }
    public string trang_thai { get; set; } = null!;
}
