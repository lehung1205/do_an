namespace JobPortal.API.DTOs;

/// <summary>Thông tin nhà tuyển dụng kèm điểm đánh giá trung bình từ ứng viên.</summary>
public class EmployerWithRatingDto
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? Image { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public byte? Gender { get; set; }
    public double? AverageRating { get; set; }
    public int ReviewCount { get; set; }
}
