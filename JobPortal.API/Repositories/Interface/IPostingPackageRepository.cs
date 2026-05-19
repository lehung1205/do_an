using JobPortal.API.Models;

namespace JobPortal.API.Repositories.Interface;

public interface IPostingPackageRepository
{
    Task<IReadOnlyList<PostingPackage>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PostingPackage?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task AddAsync(PostingPackage entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(PostingPackage entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default);
}
