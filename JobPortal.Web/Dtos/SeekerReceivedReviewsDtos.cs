namespace JobPortal.Web.Dtos;

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
