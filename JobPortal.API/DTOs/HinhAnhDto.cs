namespace JobPortal.API.DTOs;

public class HinhAnhDto
{
    public long id_hinh_anh { get; set; }
    public long id_cong_viec { get; set; }
    public string duong_dan_url { get; set; } = null!;
    public string? ten { get; set; }
}
