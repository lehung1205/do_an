namespace JobPortal.API.DTOs;

public class HoSoTuyenDungDto
{
    public long id_cv { get; set; }
    public long id_tim_viec { get; set; }
    public string tieu_de { get; set; } = null!;
    public string url { get; set; } = null!;
    public DateTime ngay_tao { get; set; }
}
