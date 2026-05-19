namespace JobPortal.API.DTOs;

public class CreateAdminDto
{
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string? Phone { get; set; }
    public string? AccountNumber { get; set; }
    public string? BankName { get; set; }
    public string Status { get; set; } = "ACTIVE";
}
