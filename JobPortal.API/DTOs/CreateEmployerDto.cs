namespace JobPortal.API.DTOs;

public class CreateEmployerDto
{
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string? Phone { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public byte? Gender { get; set; }
    public string? Description { get; set; }
    public string? Image { get; set; }
    public int PostingLimit { get; set; } = 1;
    public string? IdCard { get; set; }
    public string Status { get; set; } = "ACTIVE";
}
