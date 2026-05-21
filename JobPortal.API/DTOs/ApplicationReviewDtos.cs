namespace JobPortal.API.DTOs;

public class ApplicationReviewViewDto
{
    public long Id { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public string ReviewType { get; set; } = null!;
    public string ReviewerName { get; set; } = null!;
}

public class ApplicationReviewContextDto
{
    public long ApplicationId { get; set; }
    public bool IsWorkFinished { get; set; }
    public bool CanSubmitReview { get; set; }
    public string TargetLabel { get; set; } = null!;
    public ApplicationReviewViewDto? MyReview { get; set; }
    public ApplicationReviewViewDto? ReceivedReview { get; set; }
}

public class CreateApplicationReviewRequest
{
    public int Rating { get; set; }
    public string? Comment { get; set; }
}

public class SeekerReceivedReviewItemDto
{
    public long Id { get; set; }
    public long ApplicationId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public string EmployerName { get; set; } = null!;
    public string JobTitle { get; set; } = null!;
}

public class SeekerReceivedReviewsSummaryDto
{
    public double? AverageRating { get; set; }
    public int TotalCount { get; set; }
    public IReadOnlyList<SeekerReceivedReviewItemDto> Items { get; set; } = Array.Empty<SeekerReceivedReviewItemDto>();
}

public class EmployerReceivedReviewItemDto
{
    public long Id { get; set; }
    public long ApplicationId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public string ApplicantName { get; set; } = null!;
    public string JobTitle { get; set; } = null!;
}

public class EmployerReceivedReviewsSummaryDto
{
    public double? AverageRating { get; set; }
    public int TotalCount { get; set; }
    public IReadOnlyList<EmployerReceivedReviewItemDto> Items { get; set; } = Array.Empty<EmployerReceivedReviewItemDto>();
}
