using JobPortal.API.DTOs;
using JobPortal.API.DTOs.Common;

namespace JobPortal.API.Services.Interface;

public interface IAdminJobService
{
    Task<PagedResult<AdminPendingJobDto>> GetPendingJobsPagedAsync(
        int page,
        int pageSize,
        string? search = null,
        CancellationToken cancellationToken = default);

    Task<JobDto> ApproveJobAsync(long jobId, CancellationToken cancellationToken = default);

    Task<JobDto> RejectJobAsync(long jobId, RejectJobRequest? request, CancellationToken cancellationToken = default);
}
