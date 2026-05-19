using JobPortal.API.Models;

namespace JobPortal.API.Repositories.Interface;

public interface ICategoryRepository
{
    Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Category?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task AddAsync(Category entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Category entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default);
}
