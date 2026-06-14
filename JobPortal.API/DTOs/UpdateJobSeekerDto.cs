namespace JobPortal.API.DTOs;

public class UpdateJobSeekerDto
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateTime? EmailVerifiedAt { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public byte? Gender { get; set; }
    public string? Description { get; set; }
    public string? ProfileImage { get; set; }
    public string? IdCard { get; set; }
    public string? IdCardIssueDate { get; set; }
    public string? IdCardIssuePlace { get; set; }
    public string? PermanentAddress { get; set; }
    public string? TemporaryAddress { get; set; }
    public string? Status { get; set; }
    public string? Role { get; set; }
}
