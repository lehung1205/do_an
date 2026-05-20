namespace JobPortal.Web.Dtos.Auth;

public class AuthResponse
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public DateTime AccessTokenExpiresAt { get; set; }
    public DateTime RefreshTokenExpiresAt { get; set; }
    public ProfileResponse User { get; set; } = null!;
}

public class ProfileResponse
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string Role { get; set; } = null!;
    public long? JobSeekerId { get; set; }
    public long? EmployerId { get; set; }
    public long? AdminId { get; set; }
    public string? ProfileImage { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? AccountStatus { get; set; }
    public DateTime? EmailVerifiedAt { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public byte? Gender { get; set; }
    public string? Description { get; set; }
    public string? PermanentAddress { get; set; }
    public string? TemporaryAddress { get; set; }
    public string? IdCard { get; set; }
    public string? IdCardIssueDate { get; set; }
    public string? IdCardIssuePlace { get; set; }
    public string? BankName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? ProfilePhone { get; set; }
    public int? PostingLimit { get; set; }
}
