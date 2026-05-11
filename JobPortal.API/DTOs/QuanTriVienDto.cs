namespace JobPortal.API.DTOs;

public class QuanTriVienDto
{
    public long id_quan_tri { get; set; }
    public string ten { get; set; } = null!;
    public string email { get; set; } = null!;
    public string mat_khau { get; set; } = null!;
    public string? so_tai_khoan { get; set; }
    public string? ten_ngan_hang { get; set; }
}
