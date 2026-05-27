namespace JobPortal.Web.Dtos;

public class JobRelatedListsDto
{
    public List<JobSummaryDto> SuggestedJobs { get; set; } = new();
    public List<JobSummaryDto> SameCompanyJobs { get; set; } = new();
    public List<JobSummaryDto> SimilarJobs { get; set; } = new();
}
