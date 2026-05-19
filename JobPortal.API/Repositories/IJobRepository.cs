using JobPortal.API.Models;

namespace JobPortal.API.Repositories;

public interface IJobRepository
{
    Task<(IReadOnlyList<Job> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<Job?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task AddAsync(Job entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Job entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default);
}
