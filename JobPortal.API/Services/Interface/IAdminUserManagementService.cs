using JobPortal.API.DTOs;
using JobPortal.API.DTOs.Common;

namespace JobPortal.API.Services.Interface;

public interface IAdminUserManagementService
{
    Task<PagedResult<AdminManagedEmployerDto>> GetEmployersPagedAsync(
        int page,
        int pageSize,
        string? search = null,
        string? status = null,
        CancellationToken cancellationToken = default);

    Task<PagedResult<AdminManagedJobSeekerDto>> GetJobSeekersPagedAsync(
        int page,
        int pageSize,
        string? search = null,
        string? status = null,
        CancellationToken cancellationToken = default);

    Task<AdminManagedEmployerDto> SetEmployerActiveAsync(
        long employerId,
        bool active,
        string? revokedByIp = null,
        CancellationToken cancellationToken = default);

    Task<AdminManagedJobSeekerDto> SetJobSeekerActiveAsync(
        long jobSeekerId,
        bool active,
        string? revokedByIp = null,
        CancellationToken cancellationToken = default);
}
