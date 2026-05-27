namespace JobPortal.API.DTOs;

public class JobRelatedListsDto
{
    public IReadOnlyList<JobSummaryDto> SuggestedJobs { get; set; } = Array.Empty<JobSummaryDto>();
    public IReadOnlyList<JobSummaryDto> SameCompanyJobs { get; set; } = Array.Empty<JobSummaryDto>();
    public IReadOnlyList<JobSummaryDto> SimilarJobs { get; set; } = Array.Empty<JobSummaryDto>();
}
