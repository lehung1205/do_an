namespace JobPortal.API.DTOs;

public class PaymentHistoryDto
{
    public long Id { get; set; }
    public long EmployerId { get; set; }
    public long PostingPackageId { get; set; }
    public int Amount { get; set; }
    public string OrderId { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime? PaymentDate { get; set; }
    public string? PaymentBank { get; set; }
    public string? TransactionCode { get; set; }
}
