namespace JobPortal.Web.Dtos;

public class JobRelatedListsDto
{
    public List<JobDto> SuggestedJobs { get; set; } = new();
    public List<JobDto> SameCompanyJobs { get; set; } = new();
    public List<JobDto> SimilarJobs { get; set; } = new();
}
