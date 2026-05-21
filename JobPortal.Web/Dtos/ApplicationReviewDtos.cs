namespace JobPortal.Web.Dtos;

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
