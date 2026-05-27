using System.Globalization;
using JobPortal.Web.Dtos;
using JobPortal.Web.Dtos.Common;
using JobPortal.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JobPortal.Web.Pages.Admin.Payments;

public class IndexModel : PageModel
{
    public const int DefaultPageSize = 15;
    public const string StatusAll = "all";
    public const string StatusPaid = "paid";
    public const string StatusPending = "pending";
    public const string StatusFailed = "failed";

    private readonly ApiService _api;

    public IndexModel(ApiService api) => _api = api;

    public AdminPaymentRevenueDto? Revenue { get; set; }
    public List<AdminPaymentListItemDto> Payments { get; set; } = new();
    public string? ErrorMessage { get; set; }
    public string? Search { get; set; }
    public string StatusFilter { get; set; } = StatusAll;
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = DefaultPageSize;
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool ShowPagination => TotalPages > 1;
    [TempData]
    public string? ActionErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(
        string? status,
        string? q,
        DateTime? from,
        DateTime? to,
        int pageNumber = 1,
        int pageSize = DefaultPageSize)
    {
        var redirect = RequireAdmin();
        if (redirect != null)
        {
            return redirect;
        }

        if (pageNumber < 1)
        {
            pageNumber = 1;
        }

        if (pageSize < 1)
        {
            pageSize = DefaultPageSize;
        }

        StatusFilter = NormalizeStatusFilter(status);
        Search = string.IsNullOrWhiteSpace(q) ? null : q.Trim();
        FromDate = from?.Date;
        ToDate = to?.Date;

        Revenue = await _api.GetApiDataAsync<AdminPaymentRevenueDto>("/api/admin/payments/revenue?months=6");
        if (Revenue == null)
        {
            ErrorMessage = "Không tải được dữ liệu doanh thu.";
        }

        var query = $"page={pageNumber}&pageSize={pageSize}&status={Uri.EscapeDataString(StatusFilter)}";
        if (!string.IsNullOrEmpty(Search))
        {
            query += $"&q={Uri.EscapeDataString(Search)}";
        }

        if (FromDate.HasValue)
        {
            query += $"&from={FromDate.Value:yyyy-MM-dd}";
        }

        if (ToDate.HasValue)
        {
            query += $"&to={ToDate.Value:yyyy-MM-dd}";
        }

        var paged = await _api.GetApiDataAsync<PagedResult<AdminPaymentListItemDto>>(
            $"/api/admin/payments/history?{query}");

        if (paged == null)
        {
            ErrorMessage = string.IsNullOrEmpty(ErrorMessage)
                ? "Không tải được lịch sử mua gói."
                : ErrorMessage + " Không tải được lịch sử mua gói.";
            return Page();
        }

        Payments = paged.Items.ToList();
        CurrentPage = paged.Page > 0 ? paged.Page : pageNumber;
        PageSize = paged.PageSize > 0 ? paged.PageSize : pageSize;
        TotalCount = paged.TotalCount;
        TotalPages = paged.TotalPages > 0
            ? paged.TotalPages
            : TotalCount == 0
                ? 0
                : (int)Math.Ceiling(TotalCount / (double)PageSize);

        return Page();
    }

    public async Task<IActionResult> OnGetExportInvoiceAsync(
        long id,
        string? status,
        string? q,
        DateTime? from,
        DateTime? to,
        int pageNumber = 1,
        int pageSize = DefaultPageSize)
    {
        var redirect = RequireAdmin();
        if (redirect != null)
        {
            return redirect;
        }

        var response = await _api.GetRawAsync($"/api/admin/payments/{id}/invoice");
        if (!response.IsSuccessStatusCode)
        {
            ActionErrorMessage = "Không thể xuất hóa đơn. Vui lòng thử lại.";
            return RedirectToPage("/Admin/Payments/Index", new { status, q, from, to, pageNumber, pageSize });
        }

        var bytes = await response.Content.ReadAsByteArrayAsync();
        var fileName = response.Content.Headers.ContentDisposition?.FileNameStar
            ?? response.Content.Headers.ContentDisposition?.FileName
            ?? $"hoa-don-{id}.pdf";

        return File(bytes, response.Content.Headers.ContentType?.ToString() ?? "application/pdf", fileName.Trim('"'));
    }

    public async Task<IActionResult> OnGetExportRevenueExcelAsync(DateTime? from, DateTime? to)
    {
        var redirect = RequireAdmin();
        if (redirect != null)
        {
            return redirect;
        }

        var query = string.Empty;
        if (from.HasValue || to.HasValue)
        {
            var parts = new List<string>();
            if (from.HasValue)
            {
                parts.Add($"from={from.Value:yyyy-MM-dd}");
            }

            if (to.HasValue)
            {
                parts.Add($"to={to.Value:yyyy-MM-dd}");
            }

            query = "?" + string.Join("&", parts);
        }

        var response = await _api.GetRawAsync($"/api/admin/payments/revenue/export-excel{query}");
        if (!response.IsSuccessStatusCode)
        {
            ActionErrorMessage = "Không thể xuất file doanh thu.";
            return RedirectToPage("/Admin/Payments/Index", new { from, to });
        }

        var bytes = await response.Content.ReadAsByteArrayAsync();
        var fileName = response.Content.Headers.ContentDisposition?.FileNameStar
            ?? response.Content.Headers.ContentDisposition?.FileName
            ?? "doanh-thu.xlsx";

        return File(bytes, response.Content.Headers.ContentType?.ToString() ?? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName.Trim('"'));
    }

    public static string FormatMoney(long amount) =>
        amount.ToString("N0", CultureInfo.GetCultureInfo("vi-VN")) + " ₫";

    public static string FormatDateTime(DateTime? value) =>
        value.HasValue
            ? value.Value.ToLocalTime().ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)
            : "—";

    public string StatusLabel(string status) => status.Trim().ToLowerInvariant() switch
    {
        StatusPaid => "Đã thanh toán",
        StatusPending => "Chờ thanh toán",
        StatusFailed => "Thất bại",
        _ => status
    };

    public string StatusBadgeClass(string status) => status.Trim().ToLowerInvariant() switch
    {
        StatusPaid => "bg-success-subtle text-success-emphasis",
        StatusPending => "bg-warning-subtle text-warning-emphasis",
        StatusFailed => "bg-danger-subtle text-danger-emphasis",
        _ => "bg-secondary-subtle text-secondary-emphasis"
    };

    private static string NormalizeStatusFilter(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return StatusAll;
        }

        var s = status.Trim().ToLowerInvariant();
        return s is StatusPaid or StatusPending or StatusFailed or StatusAll ? s : StatusAll;
    }

    private IActionResult? RequireAdmin()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken")))
        {
            return RedirectToPage("/Auth/Login", new
            {
                returnUrl = HttpContext.Request.Path + HttpContext.Request.QueryString
            });
        }

        if (!string.Equals(HttpContext.Session.GetString("UserRole"), "ADMIN", StringComparison.Ordinal))
        {
            return RedirectToPage("/Index");
        }

        return null;
    }
}
