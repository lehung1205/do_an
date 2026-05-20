using JobPortal.API.Models;

namespace JobPortal.API.Repositories.Interface;

public interface IResumeRepository
{
    Task<IReadOnlyList<Resume>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Resume>> GetByJobSeekerIdAsync(long jobSeekerId, CancellationToken cancellationToken = default);
    Task<Resume?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task AddAsync(Resume entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Resume entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default);
}
