using JobPortal.API.DTOs;
using JobPortal.API.DTOs.Common;

namespace JobPortal.API.Services.Interface;

public interface IEmployerDashboardService
{
    Task<EmployerDashboardDto> GetDashboardForUserAsync(long userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EmployerDashboardApplicationDto>> GetApplicationsForUserAsync(
        long userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EmployerDashboardJobDto>> GetJobsForUserAsync(
        long userId,
        CancellationToken cancellationToken = default);

    Task<EmployerDashboardApplicationDto> UpdateApplicationStatusAsync(
        long userId,
        long applicationId,
        UpdateEmployerApplicationStatusRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WorkProgressJobOptionDto>> GetWorkProgressJobOptionsAsync(
        long userId,
        CancellationToken cancellationToken = default);

    Task<PagedResult<EmployerAcceptedApplicationDto>> GetAcceptedApplicationsWithProgressAsync(
        long userId,
        long? jobId = null,
        string? search = null,
        int page = 1,
        int pageSize = 9,
        CancellationToken cancellationToken = default);

    Task<ApplicationWorkProgressDto> GetApplicationWorkProgressAsync(
        long userId,
        long applicationId,
        CancellationToken cancellationToken = default);

    Task<WorkProgressStepDto> AddWorkProgressStepAsync(
        long userId,
        long applicationId,
        CreateWorkProgressStepRequest request,
        CancellationToken cancellationToken = default);
}
