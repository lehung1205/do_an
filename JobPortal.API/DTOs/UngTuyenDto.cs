namespace JobPortal.API.DTOs;

public class UngTuyenDto
{
    public long id_ung_tuyen { get; set; }
    public long id_tim_viec { get; set; }
    public long id_cong_viec { get; set; }
    public long id_cv { get; set; }
    public DateTime ngay_ung_tuyen { get; set; }
    public string trang_thai { get; set; } = null!;
}
