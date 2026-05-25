using JobPortal.API.DTOs;
using JobPortal.API.DTOs.Common;

namespace JobPortal.API.Services.Interface;

public interface IJobService
{
    Task<PagedResult<JobDto>> GetJobsPagedAsync(
        int page,
        int pageSize,
        string? search = null,
        string? location = null,
        long? categoryId = null,
        CancellationToken cancellationToken = default);
    Task<JobDto> GetJobByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<JobRelatedListsDto> GetRelatedJobsAsync(long id, CancellationToken cancellationToken = default);
    Task<JobDto> CreateJobAsync(CreateJobRequest request, CancellationToken cancellationToken = default);
    Task UpdateJobAsync(long id, JobDto jobDto, CancellationToken cancellationToken = default);
    Task DeleteJobAsync(long id, CancellationToken cancellationToken = default);
}
