namespace JobPortal.Web.Dtos;

public class AdminPaymentRevenueDto
{
    public long TotalRevenue { get; set; }
    public long MonthRevenue { get; set; }
    public long TodayRevenue { get; set; }
    public int PaidTransactionCount { get; set; }
    public int PendingTransactionCount { get; set; }
    public int FailedTransactionCount { get; set; }
    public int TotalTransactionCount { get; set; }
    public List<AdminChartPointDto> MonthlyRevenue { get; set; } = new();
    public List<AdminPaymentStatusCountDto> ByStatus { get; set; } = new();
}

public class AdminPaymentStatusCountDto
{
    public string Status { get; set; } = "";
    public int Count { get; set; }
    public long AmountSum { get; set; }
}

public class AdminPaymentListItemDto
{
    public long Id { get; set; }
    public long EmployerId { get; set; }
    public string EmployerName { get; set; } = "";
    public string EmployerEmail { get; set; } = "";
    public string PackageName { get; set; } = "";
    public int Amount { get; set; }
    public string Currency { get; set; } = "VND";
    public string OrderId { get; set; } = "";
    public string Status { get; set; } = "";
    public string? PaymentProvider { get; set; }
    public string? ProviderTransactionId { get; set; }
    public string? TransactionCode { get; set; }
    public DateTime? PaymentDate { get; set; }
    public DateTime CreatedAt { get; set; }
}
