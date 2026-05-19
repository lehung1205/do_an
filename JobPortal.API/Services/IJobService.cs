using JobPortal.API.DTOs;
using JobPortal.API.DTOs.Common;

namespace JobPortal.API.Services;

public interface IJobService
{
    Task<PagedResult<JobDto>> GetJobsPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<JobDto> GetJobByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<JobDto> CreateJobAsync(JobDto jobDto, CancellationToken cancellationToken = default);
    Task UpdateJobAsync(long id, JobDto jobDto, CancellationToken cancellationToken = default);
    Task DeleteJobAsync(long id, CancellationToken cancellationToken = default);
}
