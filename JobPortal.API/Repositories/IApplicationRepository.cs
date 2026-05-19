using JobPortal.API.Models;

namespace JobPortal.API.Repositories;

public interface IApplicationRepository
{
    Task<IReadOnlyList<Application>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Application?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task AddAsync(Application entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Application entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default);
}
