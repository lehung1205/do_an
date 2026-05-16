namespace JobPortal.API.DTOs;

public class EmployerDto
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public DateTime? EmailVerifiedAt { get; set; }
    public string PasswordHash { get; set; } = null!;
    public DateOnly? DateOfBirth { get; set; }
    public byte? Gender { get; set; }
    public string? Description { get; set; }
    public string? Image { get; set; }
    public int PostingLimit { get; set; }
    public string? IdCard { get; set; }
    public string? Phone { get; set; }
    public string Status { get; set; } = null!;
}
