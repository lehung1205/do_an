using JobPortal.API.Models;

namespace JobPortal.API.Repositories;

public interface IProcessRepository
{
    Task<IReadOnlyList<Process>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Process?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task AddAsync(Process entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Process entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default);
}
