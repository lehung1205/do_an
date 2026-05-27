using JobPortal.API.DTOs;

namespace JobPortal.API.Services.Interface;

public interface IAdminPostingPackageService
{
    Task<IReadOnlyList<AdminPostingPackageDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<AdminPostingPackageDto> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<AdminPostingPackageDto> CreateAsync(long adminUserId, CreateAdminPostingPackageRequest request, CancellationToken cancellationToken = default);
    Task<AdminPostingPackageDto> UpdateAsync(long id, UpdateAdminPostingPackageRequest request, CancellationToken cancellationToken = default);
    Task<AdminPostingPackageDto> SetActiveAsync(long id, bool isActive, CancellationToken cancellationToken = default);
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
}
