using JobPortal.API.Models;
using JobPortal.API.Models.Auth;

namespace JobPortal.API.Repositories;

public interface IEmployerRepository
{
    Task<IReadOnlyList<Employer>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Employer?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<Employer?> GetByIdWithUserAsync(long id, CancellationToken cancellationToken = default);
    Task<bool> UserExistsAsync(string email, string? phoneNumber, CancellationToken cancellationToken = default);
    Task AddWithUserAsync(User user, Employer employer, CancellationToken cancellationToken = default);
    Task UpdateAsync(Employer employer, User user, CancellationToken cancellationToken = default);
    Task DeleteWithUserAsync(Employer employer, CancellationToken cancellationToken = default);
}
