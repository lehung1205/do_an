namespace JobPortal.API.DTOs;

public class VnPayPaymentResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public long PaymentHistoryId { get; set; }
    public int Amount { get; set; }
    public string? TransactionId { get; set; }
    public string? ResponseCode { get; set; }
}

