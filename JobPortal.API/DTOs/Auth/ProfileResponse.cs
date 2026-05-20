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
}
