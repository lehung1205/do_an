using JobPortal.API.Models;

namespace JobPortal.API.Repositories.Interface;

public interface IApplicationRepository
{
    Task<IReadOnlyList<Application>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Application?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task AddAsync(Application entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Application entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default);
    /// <summary>Removes all applications submitted with this resume (required before deleting the resume row).</summary>
    Task DeleteByResumeIdAsync(long resumeId, CancellationToken cancellationToken = default);
}
