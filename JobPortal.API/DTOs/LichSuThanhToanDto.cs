namespace JobPortal.API.DTOs;

public class LichSuThanhToanDto
{
    public long id_thanh_toan { get; set; }
    public long id_tuyen_dung { get; set; }
    public long id_goi { get; set; }
    public int gia { get; set; }
    public string id_don_hang { get; set; } = null!;
    public string trang_thai { get; set; } = null!;
    public DateTime? ngay_thanh_toan { get; set; }
    public string? ngan_hang_thanh_toan { get; set; }
    public string? ma_giao_dich { get; set; }
}
