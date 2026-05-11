namespace JobPortal.Web.Dtos;

public class CongViecDto
{
    public long id_cong_viec { get; set; }
    public long id_tuyen_dung { get; set; }
    public long id_danh_muc { get; set; }
    public string tieu_de { get; set; } = null!;
    public string mo_ta { get; set; } = null!;
    public int muc_luong { get; set; }
    public string dia_diem { get; set; } = null!;
    public string trang_thai_bai_dang { get; set; } = null!;
    public DateTime ngay_bat_dau { get; set; }
    public DateTime ngay_ket_thuc { get; set; }
    public DateTime ngay_het_han { get; set; }
}
