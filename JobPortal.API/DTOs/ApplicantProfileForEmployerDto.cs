namespace JobPortal.API.DTOs;

/// <summary>Thông tin ứng viên cho nhà tuyển dụng xem khi duyệt CV.</summary>
public class ApplicantProfileForEmployerDto
{
    public long ApplicationId { get; set; }
    public string Name { get; set; } = null!;
    public string? ProfileImage { get; set; }
    public string Email { get; set; } = null!;
    public string? Phone { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public byte? Gender { get; set; }
    public string? Description { get; set; }
    public string? PermanentAddress { get; set; }
    public string? TemporaryAddress { get; set; }
    public string JobTitle { get; set; } = null!;
    public DateTime AppliedAt { get; set; }
    public string ResumeTitle { get; set; } = null!;
    public string ApplicationStatus { get; set; } = null!;
}
