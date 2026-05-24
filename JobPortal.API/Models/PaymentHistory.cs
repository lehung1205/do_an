using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobPortal.API.Models;

[Table("payment_histories")]
public class PaymentHistory
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("employer_id")]
    public long EmployerId { get; set; }

    [ForeignKey(nameof(EmployerId))]
    public Employer Employer { get; set; } = null!;

    [Column("posting_package_id")]
    public long PostingPackageId { get; set; }

    [ForeignKey(nameof(PostingPackageId))]
    public PostingPackage PostingPackage { get; set; } = null!;

    [Column("amount")]
    public int Amount { get; set; }

    [Column("currency")]
    [MaxLength(10)]
    public string Currency { get; set; } = "VND";

    [Column("order_id")]
    [MaxLength(100)]
    public string OrderId { get; set; } = null!;

    [Column("status")]
    [MaxLength(32)]
    public string Status { get; set; } = null!;

    [Column("payment_date")]
    public DateTime? PaymentDate { get; set; }

    [Column("payment_bank")]
    [MaxLength(255)]
    public string? PaymentBank { get; set; }

    [Column("payment_provider")]
    [MaxLength(100)]
    public string? PaymentProvider { get; set; }

    [Column("transaction_code")]
    [MaxLength(100)]
    public string? TransactionCode { get; set; }

    [Column("provider_transaction_id")]
    [MaxLength(100)]
    public string? ProviderTransactionId { get; set; }

    [Column("package_name_snapshot")]
    [MaxLength(255)]
    public string? PackageNameSnapshot { get; set; }

    [Column("price_snapshot")]
    public int? PriceSnapshot { get; set; }

    [Column("posting_limit_snapshot")]
    public int? PostingLimitSnapshot { get; set; }

    [Column("expired_at")]
    public DateTime? ExpiredAt { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}
