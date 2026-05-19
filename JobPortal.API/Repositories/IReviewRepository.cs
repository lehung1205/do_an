using JobPortal.API.Models;

namespace JobPortal.API.Repositories;

public interface IReviewRepository
{
    Task<IReadOnlyList<Review>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Review?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task AddAsync(Review entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Review entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default);
}
