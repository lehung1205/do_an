using JobPortal.API.DTOs;

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
}
