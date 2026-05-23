using JobPortal.API.Models;

namespace JobPortal.API.Repositories.Interface;

public interface IJobRepository
{
    Task<(IReadOnlyList<Job> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        bool recruitingOnly = false,
        string? search = null,
        string? location = null,
        CancellationToken cancellationToken = default);
    Task<int> CloseExpiredRecruitingJobsAsync(CancellationToken cancellationToken = default);
    Task<int> AutoApproveStalePendingJobsAsync(TimeSpan pendingMaxAge, CancellationToken cancellationToken = default);
    Task<Job?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task AddAsync(Job entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Job entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default);
}
