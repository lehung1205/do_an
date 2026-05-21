namespace JobPortal.Web.Dtos;

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
