using JobPortal.API.DTOs;

namespace JobPortal.API.Services;

public interface IAdminService
{
    Task<IReadOnlyList<AdminDto>> GetAllAdminsAsync(CancellationToken cancellationToken = default);
    Task<AdminDto> GetAdminByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<AdminDto> CreateAdminAsync(CreateAdminDto dto, CancellationToken cancellationToken = default);
    Task UpdateAdminAsync(long id, UpdateAdminDto dto, CancellationToken cancellationToken = default);
    Task DeleteAdminAsync(long id, CancellationToken cancellationToken = default);
}
