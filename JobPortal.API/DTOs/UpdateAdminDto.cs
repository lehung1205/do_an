namespace JobPortal.API.DTOs;

/// <summary>
/// Partial update — only sent fields are applied.
/// </summary>
public class UpdateAdminDto
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? AccountNumber { get; set; }
    public string? BankName { get; set; }
    public string? Status { get; set; }
    public string? Role { get; set; }
}
