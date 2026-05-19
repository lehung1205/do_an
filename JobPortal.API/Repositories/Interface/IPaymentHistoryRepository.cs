using JobPortal.API.Models;

namespace JobPortal.API.Repositories.Interface;

public interface IPaymentHistoryRepository
{
    Task<IReadOnlyList<PaymentHistory>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PaymentHistory?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task AddAsync(PaymentHistory entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(PaymentHistory entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default);
}
