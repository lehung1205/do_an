namespace JobPortal.API.DTOs;

public class VnPayPaymentResponse
{
    public bool Success { get; set; }
    public string PaymentUrl { get; set; } = string.Empty;
    public long PaymentHistoryId { get; set; }
    public string OrderId { get; set; } = string.Empty;
    public int Amount { get; set; }
    public string TxnRef { get; set; } = string.Empty;
}
