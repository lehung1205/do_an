namespace JobPortal.Web.Dtos.Auth;

public class UpdateProfileRequest
{
    public string Name { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string? ProfileImage { get; set; }

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
}
