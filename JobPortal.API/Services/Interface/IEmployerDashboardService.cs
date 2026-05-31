using JobPortal.API.DTOs;
using JobPortal.API.DTOs.Common;

namespace JobPortal.API.Services.Interface;

public interface IEmployerDashboardService
{
    Task<EmployerDashboardDto> GetDashboardForUserAsync(long userId, CancellationToken cancellationToken = default);
    Task<EmployerApplicantChartDto> GetApplicantChartForUserAsync(
        long userId,
        int days = 7,
        CancellationToken cancellationToken = default);
    Task<EmployerPortalNavDto> GetPortalNavForUserAsync(long userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EmployerDashboardApplicationDto>> GetApplicationsForUserAsync(
        long userId,
        CancellationToken cancellationToken = default);

    Task<ApplicantProfileForEmployerDto> GetApplicantProfileForEmployerAsync(
        long userId,
        long applicationId,
        CancellationToken cancellationToken = default);

    Task<PagedResult<EmployerDashboardJobDto>> GetJobsForUserAsync(
        long userId,
        string? view = null,
        string? search = null,
        int page = 1,
        int pageSize = 9,
        CancellationToken cancellationToken = default);

    Task<EmployerDashboardJobDto> CloseJobForUserAsync(
        long userId,
        long jobId,
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
        string? progress = null,
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
