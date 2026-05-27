using JobPortal.API.DTOs;
using JobPortal.API.DTOs.Common;

namespace JobPortal.API.Services.Interface;

public interface IAdminPaymentService
{
    Task<AdminPaymentRevenueDto> GetRevenueAsync(int months = 6, CancellationToken cancellationToken = default);
    Task<PagedResult<AdminPaymentListItemDto>> GetPaymentHistoryPagedAsync(
        int page,
        int pageSize,
        string? status = null,
        string? search = null,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default);
    Task<(byte[] Content, string ContentType, string FileName)> GenerateInvoiceFileAsync(
        long paymentId,
        CancellationToken cancellationToken = default);
    Task<(byte[] Content, string ContentType, string FileName)> ExportRevenueExcelAsync(
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default);
}
