namespace JobPortal.API.DTOs;

public class AdminDto
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? AccountNumber { get; set; }
    public string? BankName { get; set; }
    public string? Phone { get; set; }
    public string Status { get; set; } = null!;
    public string Role { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
