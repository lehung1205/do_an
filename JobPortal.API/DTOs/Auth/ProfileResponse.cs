namespace JobPortal.API.DTOs.Auth;

public class ProfileResponse
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string Role { get; set; } = null!;

    /// <summary>Present when the user has a job seeker profile (same row as <c>job_seekers</c>).</summary>
    public long? JobSeekerId { get; set; }

    /// <summary>Present when the user has an employer profile (same row as <c>employers</c>).</summary>
    public long? EmployerId { get; set; }

    /// <summary>Present when the user has an admin profile.</summary>
    public long? AdminId { get; set; }

    /// <summary>Avatar URL (stored on <c>users.profile_image</c>; may mirror job seeker / employer).</summary>
    public string? ProfileImage { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    /// <summary>Status from role profile (e.g. ACTIVE, recruiting context).</summary>
    public string? AccountStatus { get; set; }

    public DateTime? EmailVerifiedAt { get; set; }
    public DateOnly? DateOfBirth { get; set; }

    /// <summary>0 = other, 1 = male, 2 = female (convention; adjust if your seed differs).</summary>
    public byte? Gender { get; set; }

    public string? Description { get; set; }
    public string? PermanentAddress { get; set; }
    public string? TemporaryAddress { get; set; }
    public string? IdCard { get; set; }
    public string? IdCardIssueDate { get; set; }
    public string? IdCardIssuePlace { get; set; }
    public string? BankName { get; set; }
    public string? BankAccountNumber { get; set; }

    public int? PostingLimit { get; set; }
}
