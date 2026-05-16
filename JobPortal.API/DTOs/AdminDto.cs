namespace JobPortal.API.DTOs;

public class AdminDto
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string? AccountNumber { get; set; }
    public string? BankName { get; set; }
}
