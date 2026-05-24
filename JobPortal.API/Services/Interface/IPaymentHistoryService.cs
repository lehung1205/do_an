using JobPortal.API.DTOs;

namespace JobPortal.API.Services.Interface;

public interface IPaymentHistoryService
{
    Task<IReadOnlyList<PaymentHistoryDto>> GetAllPaymentHistoriesAsync(CancellationToken cancellationToken = default);
    Task<PaymentHistoryDto> GetPaymentHistoryByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<PaymentHistoryDto> CreatePaymentHistoryAsync(PaymentHistoryDto dto, CancellationToken cancellationToken = default);
    Task<PaymentHistoryDto> CreatePendingPackagePaymentAsync(long userId, long postingPackageId, CancellationToken cancellationToken = default);
    Task<VnPayPaymentResult> ConfirmVnPayPaymentAsync(
        long paymentHistoryId,
        bool isSuccessful,
        string? providerTransactionId,
        string? bankCode,
        string? bankTransactionCode,
        string? responseCode,
        CancellationToken cancellationToken = default);
    Task UpdatePaymentHistoryAsync(long id, PaymentHistoryDto dto, CancellationToken cancellationToken = default);
    Task DeletePaymentHistoryAsync(long id, CancellationToken cancellationToken = default);
}
