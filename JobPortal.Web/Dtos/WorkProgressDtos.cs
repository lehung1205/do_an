using JobPortal.Web.Dtos.Common;

namespace JobPortal.Web.Dtos;

public class WorkProgressStepDto
{
    public long Id { get; set; }
    public long ApplicationId { get; set; }
    public string Status { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class WorkProgressJobOptionDto
{
    public long JobId { get; set; }
    public string JobTitle { get; set; } = null!;
    public int AcceptedCount { get; set; }
}

public class EmployerWorkProgressListDto
{
    public PagedResult<EmployerAcceptedApplicationDto> Applications { get; set; } = new();
    public IReadOnlyList<WorkProgressJobOptionDto> JobOptions { get; set; } = Array.Empty<WorkProgressJobOptionDto>();
}

public class EmployerAcceptedApplicationDto
{
    public long ApplicationId { get; set; }
    public long JobId { get; set; }
    public string ApplicantName { get; set; } = null!;
    public string? ApplicantProfileImage { get; set; }
    public string JobTitle { get; set; } = null!;
    public DateTime AppliedAt { get; set; }
    public string? CurrentWorkStatus { get; set; }
    public string? CurrentWorkTitle { get; set; }
    public DateTime? LastProgressAt { get; set; }
    public int StepCount { get; set; }
    public bool IsProgressLocked { get; set; }
    public bool IsWorkFinished { get; set; }
    public bool HasSubmittedReview { get; set; }
}

public class ApplicationWorkProgressDto
{
    public long ApplicationId { get; set; }
    public long JobId { get; set; }
    public string ApplicantName { get; set; } = null!;
    public string? ApplicantProfileImage { get; set; }
    public string? ApplicantEmail { get; set; }
    public string? ApplicantPhone { get; set; }
    public string JobTitle { get; set; } = null!;
    public DateTime AppliedAt { get; set; }
    public string ApplicationStatus { get; set; } = null!;
    public IReadOnlyList<WorkProgressStepDto> Steps { get; set; } = Array.Empty<WorkProgressStepDto>();
    public WorkProgressStepDto? CurrentStep { get; set; }
    public bool IsProgressLocked { get; set; }
}

public class CreateWorkProgressStepRequest
{
    public string Status { get; set; } = null!;
    public string? Notes { get; set; }
}

public class SeekerWorkProgressListItemDto
{
    public long ApplicationId { get; set; }
    public long JobId { get; set; }
    public string JobTitle { get; set; } = null!;
    public string CompanyName { get; set; } = null!;
    public string JobLocation { get; set; } = null!;
    public DateTime AppliedAt { get; set; }
    public string? CurrentWorkStatus { get; set; }
    public string? CurrentWorkTitle { get; set; }
    public DateTime? LastProgressAt { get; set; }
    public int StepCount { get; set; }
    public bool IsProgressLocked { get; set; }
    public bool IsWorkFinished { get; set; }
    public bool HasSubmittedReview { get; set; }
}

public class SeekerApplicationWorkProgressDto
{
    public long ApplicationId { get; set; }
    public long JobId { get; set; }
    public string JobTitle { get; set; } = null!;
    public string CompanyName { get; set; } = null!;
    public string JobLocation { get; set; } = null!;
    public string JobSalary { get; set; } = null!;
    public DateTime AppliedAt { get; set; }
    public string ApplicationStatus { get; set; } = null!;
    public IReadOnlyList<WorkProgressStepDto> Steps { get; set; } = Array.Empty<WorkProgressStepDto>();
    public WorkProgressStepDto? CurrentStep { get; set; }
    public bool IsProgressLocked { get; set; }
}
