namespace JobPortal.API.DTOs;

public class GoiBaiDangDto
{
    public long id_goi { get; set; }
    public long id_quan_tri { get; set; }
    public string ten { get; set; } = null!;
    public int gia { get; set; }
    public int so_luot_dang_bai { get; set; }
}
