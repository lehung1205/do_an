namespace JobPortal.Web.Helpers;

public static class PaymentStatusHelper
{
    public static string FormatStatus(string? status) => (status ?? "").Trim().ToLowerInvariant() switch
    {
        "paid" => "Đã thanh toán",
        "pending" => "Đang chờ",
        "failed" => "Thất bại",
        _ => string.IsNullOrWhiteSpace(status) ? "—" : status!
    };

    public static string BadgeClass(string? status) => (status ?? "").Trim().ToLowerInvariant() switch
    {
        "paid" => "emp-pay-status emp-pay-status--paid",
        "pending" => "emp-pay-status emp-pay-status--pending",
        "failed" => "emp-pay-status emp-pay-status--failed",
        _ => "emp-pay-status emp-pay-status--default"
    };
}
