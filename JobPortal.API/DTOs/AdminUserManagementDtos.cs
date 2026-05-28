namespace JobPortal.API.DTOs;

public class AdminUserManagementSummaryDto
{
    public int TotalEmployers { get; set; }
    public int ActiveEmployers { get; set; }
    public int InactiveEmployers { get; set; }
    public int TotalJobSeekers { get; set; }
    public int ActiveJobSeekers { get; set; }
    public int InactiveJobSeekers { get; set; }
}

public class AdminManagedEmployerDto
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Phone { get; set; }
    public string? Image { get; set; }
    public string Status { get; set; } = null!;
    public bool EmailVerified { get; set; }
    public int PostingLimit { get; set; }
    public int JobCount { get; set; }
    public int OpenJobCount { get; set; }
    public int ApplicantCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class AdminManagedJobSeekerDto
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Phone { get; set; }
    public string? ProfileImage { get; set; }
    public string Status { get; set; } = null!;
    public bool EmailVerified { get; set; }
    public int ApplicationCount { get; set; }
    public int AcceptedApplicationCount { get; set; }
    public int ResumeCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class SetAccountActiveRequest
{
    public bool Active { get; set; }
}
