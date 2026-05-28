using JobPortal.API.DTOs;
using JobPortal.API.DTOs.Common;

namespace JobPortal.API.Services.Interface;

public interface IAdminJobService
{
    Task<PagedResult<AdminPendingJobDto>> GetJobsPagedAsync(
        int page,
        int pageSize,
        string? status = null,
        string? search = null,
        CancellationToken cancellationToken = default);

    Task<JobDto> ApproveJobAsync(long jobId, CancellationToken cancellationToken = default);

    Task<JobDto> RejectJobAsync(long jobId, RejectJobRequest? request, CancellationToken cancellationToken = default);

    Task<(byte[] Content, string ContentType, string FileName)> ExportJobsByCategoryExcelAsync(
        CancellationToken cancellationToken = default);

    Task<(byte[] Content, string ContentType, string FileName)> ExportJobsListExcelAsync(
        string? status = null,
        string? search = null,
        CancellationToken cancellationToken = default);
}
