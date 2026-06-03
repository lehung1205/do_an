using JobPortal.Web.Dtos.Common;

namespace JobPortal.Web.Dtos;

public class EmployerPaymentSummaryDto
{
    public long TotalPaidAmount { get; set; }
    public int PaidCount { get; set; }
    public int PendingCount { get; set; }
    public int FailedCount { get; set; }
    public int TotalCount { get; set; }
    public int CurrentPostingLimit { get; set; }
}

public class EmployerPaymentListItemDto
{
    public long Id { get; set; }
    public string PackageName { get; set; } = "";
    public int? PostingLimitSnapshot { get; set; }
    public int Amount { get; set; }
    public string Currency { get; set; } = "VND";
    public string OrderId { get; set; } = "";
    public string Status { get; set; } = "";
    public string? PaymentProvider { get; set; }
    public string? PaymentBank { get; set; }
    public string? ProviderTransactionId { get; set; }
    public string? TransactionCode { get; set; }
    public DateTime? PaymentDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class EmployerPaymentHistoryResultDto
{
    public EmployerPaymentSummaryDto Summary { get; set; } = new();
    public PagedResult<EmployerPaymentListItemDto> Payments { get; set; } = new();
}
