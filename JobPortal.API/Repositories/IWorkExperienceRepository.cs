using JobPortal.API.Models;

namespace JobPortal.API.Repositories;

public interface IWorkExperienceRepository
{
    Task<IReadOnlyList<WorkExperience>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<WorkExperience?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task AddAsync(WorkExperience entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(WorkExperience entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default);
}
