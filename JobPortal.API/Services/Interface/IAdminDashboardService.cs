using JobPortal.API.DTOs;

namespace JobPortal.API.Services.Interface;

public interface IAdminDashboardService
{
    Task<AdminDashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default);
}
