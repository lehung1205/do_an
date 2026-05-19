using JobPortal.API.Models;
using JobPortal.API.Models.Auth;

namespace JobPortal.API.Repositories;

public interface IAdminRepository
{
    Task<IReadOnlyList<Admin>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Admin?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<Admin?> GetByIdWithUserAsync(long id, CancellationToken cancellationToken = default);
    Task<bool> UserExistsAsync(string email, string? phoneNumber, CancellationToken cancellationToken = default);
    Task AddWithUserAsync(User user, Admin admin, CancellationToken cancellationToken = default);
    Task UpdateAsync(Admin admin, User user, CancellationToken cancellationToken = default);
    Task DeleteWithUserAsync(Admin admin, CancellationToken cancellationToken = default);
}
