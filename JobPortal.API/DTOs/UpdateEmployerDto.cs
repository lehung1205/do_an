namespace JobPortal.API.DTOs;

public class UpdateEmployerDto
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateTime? EmailVerifiedAt { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public byte? Gender { get; set; }
    public string? Description { get; set; }
    public string? Image { get; set; }
    public int? PostingLimit { get; set; }
    public string? IdCard { get; set; }
    public string? Status { get; set; }
    public string? Role { get; set; }
}
