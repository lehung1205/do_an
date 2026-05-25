namespace JobPortal.API.DTOs;

public class EmployerDashboardDto
{
    public string CompanyName { get; set; } = null!;
    public int NewApplicantsToday { get; set; }
    public int PostingLimit { get; set; }
    public EmployerDashboardStatsDto Stats { get; set; } = new();
    public IReadOnlyList<EmployerDashboardJobDto> RecentJobs { get; set; } = Array.Empty<EmployerDashboardJobDto>();
    public IReadOnlyList<EmployerDashboardApplicationDto> RecentApplications { get; set; } = Array.Empty<EmployerDashboardApplicationDto>();
    public IReadOnlyList<EmployerDashboardNotificationDto> Notifications { get; set; } = Array.Empty<EmployerDashboardNotificationDto>();
    public EmployerApplicantChartDto ApplicantChart { get; set; } = new();
}

public class EmployerDashboardStatsDto
{
    public int OpenJobs { get; set; }
    public int NewCvCount { get; set; }
    public int TotalCvCount { get; set; }
    public int ExpiringSoonCount { get; set; }
    public int UnreadApplications { get; set; }
    public int TotalJobs { get; set; }
}

public class EmployerDashboardJobDto
{
    public long Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Location { get; set; } = null!;
    public string Salary { get; set; } = null!;
    public string PostingStatus { get; set; } = null!;
    public double? EmployerAverageRating { get; set; }
    public int EmployerReviewCount { get; set; }
    public int ApplicantCount { get; set; }
    public string? WorkingHours { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string? ThumbnailUrl { get; set; }
}

public class EmployerDashboardApplicationDto
{
    public long Id { get; set; }
    public long JobId { get; set; }
    public string ApplicantName { get; set; } = null!;
    public string? ApplicantEmail { get; set; }
    public string? ApplicantPhone { get; set; }
    public string? ApplicantProfileImage { get; set; }
    public double? ApplicantAverageRating { get; set; }
    public int ApplicantReviewCount { get; set; }
    public string JobTitle { get; set; } = null!;
    public DateTime AppliedAt { get; set; }
    public long ResumeId { get; set; }
    public string ResumeTitle { get; set; } = null!;
    public string? ResumeUrl { get; set; }
    public string Status { get; set; } = null!;
    public bool IsUnread { get; set; }
}

public class EmployerDashboardNotificationDto
{
    public string Type { get; set; } = "info";
    public string Message { get; set; } = null!;
}
