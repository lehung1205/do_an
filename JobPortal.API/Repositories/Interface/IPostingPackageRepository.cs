using JobPortal.API.Models;

namespace JobPortal.API.Repositories.Interface;

public interface IPostingPackageRepository
{
    Task<IReadOnlyList<PostingPackage>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<(PostingPackage Package, int PaymentCount)>> GetAllWithPaymentCountsAsync(CancellationToken cancellationToken = default);
    Task<(PostingPackage Package, int PaymentCount)?> GetByIdWithPaymentCountAsync(long id, CancellationToken cancellationToken = default);
    Task<PostingPackage?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<int> GetPaymentCountAsync(long id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, long? excludeId, CancellationToken cancellationToken = default);
    Task AddAsync(PostingPackage entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(PostingPackage entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default);
}
