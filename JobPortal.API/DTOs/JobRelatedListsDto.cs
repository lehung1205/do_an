namespace JobPortal.API.DTOs;

public class JobRelatedListsDto
{
    public IReadOnlyList<JobDto> SuggestedJobs { get; set; } = Array.Empty<JobDto>();
    public IReadOnlyList<JobDto> SameCompanyJobs { get; set; } = Array.Empty<JobDto>();
    public IReadOnlyList<JobDto> SimilarJobs { get; set; } = Array.Empty<JobDto>();
}
