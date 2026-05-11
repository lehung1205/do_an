namespace JobPortal.API.DTOs;

public class DanhGiaDto
{
    public long id_danh_gia { get; set; }
    public long id_cong_viec { get; set; }
    public long id_tuyen_dung { get; set; }
    public long id_tim_viec { get; set; }
    public string? binh_luan { get; set; }
    public int danh_gia { get; set; }
    public string danh_gia_toi { get; set; } = null!;
}
