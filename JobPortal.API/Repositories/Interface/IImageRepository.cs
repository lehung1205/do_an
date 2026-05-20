using JobPortal.API.Models;

namespace JobPortal.API.Repositories.Interface;

public interface IImageRepository
{
    Task<IReadOnlyList<Image>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Image>> GetByJobIdAsync(long jobId, CancellationToken cancellationToken = default);
    Task<Image?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task AddAsync(Image entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Image entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default);
}
