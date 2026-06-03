using System.Globalization;
using JobPortal.Web.Dtos;
using JobPortal.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JobPortal.Web.Pages.Employer.PaymentHistory;

public class IndexModel : PageModel
{
    public const int DefaultPageSize = 10;
    public const string StatusAll = "all";

    private readonly ApiService _api;

    public IndexModel(ApiService api) => _api = api;

    public EmployerPaymentSummaryDto Summary { get; set; } = new();
    public List<EmployerPaymentListItemDto> Items { get; set; } = new();
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = DefaultPageSize;
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public string StatusFilter { get; set; } = StatusAll;
    public string? Search { get; set; }
    public string? ErrorMessage { get; set; }
    public bool ShowPagination => TotalPages > 1;

    public bool HasActiveFilter =>
        !string.Equals(StatusFilter, StatusAll, StringComparison.OrdinalIgnoreCase)
        || !string.IsNullOrEmpty(Search);

    public async Task<IActionResult> OnGetAsync(
        string? status,
        string? q,
        int pageNumber = 1,
        int pageSize = DefaultPageSize)
    {
        var redirect = RequireEmployer();
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

        StatusFilter = string.IsNullOrWhiteSpace(status) ? StatusAll : status.Trim().ToLowerInvariant();
        Search = string.IsNullOrWhiteSpace(q) ? null : q.Trim();

        var query = new List<string>
        {
            $"page={pageNumber}",
            $"pageSize={pageSize}"
        };

        if (!string.Equals(StatusFilter, StatusAll, StringComparison.OrdinalIgnoreCase))
        {
            query.Add($"status={Uri.EscapeDataString(StatusFilter)}");
        }

        if (!string.IsNullOrEmpty(Search))
        {
            query.Add($"q={Uri.EscapeDataString(Search)}");
        }

        var result = await _api.GetApiDataAsync<EmployerPaymentHistoryResultDto>(
            $"/api/payments/me/history?{string.Join("&", query)}");

        if (result == null)
        {
            ErrorMessage = "Không tải được lịch sử thanh toán.";
            return Page();
        }

        Summary = result.Summary;
        Items = result.Payments.Items.ToList();
        CurrentPage = result.Payments.Page > 0 ? result.Payments.Page : pageNumber;
        PageSize = result.Payments.PageSize > 0 ? result.Payments.PageSize : pageSize;
        TotalCount = result.Payments.TotalCount;
        TotalPages = result.Payments.TotalPages > 0
            ? result.Payments.TotalPages
            : TotalCount == 0
                ? 0
                : (int)Math.Ceiling(TotalCount / (double)PageSize);

        return Page();
    }

    public static string FormatMoney(long amount) =>
        amount.ToString("N0", CultureInfo.GetCultureInfo("vi-VN")) + " ₫";

    public static string FormatMoney(int amount) => FormatMoney((long)amount);

    public static string FormatDateTime(DateTime? utc) =>
        utc.HasValue ? utc.Value.ToLocalTime().ToString("dd/MM/yyyy HH:mm") : "—";

    public static string FormatDateTime(DateTime utc) =>
        utc.ToLocalTime().ToString("dd/MM/yyyy HH:mm");

    private IActionResult? RequireEmployer()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken")))
        {
            return RedirectToPage("/Auth/Login", new { returnUrl = Url.Page("/Employer/PaymentHistory/Index") });
        }

        if (!string.Equals(HttpContext.Session.GetString("UserRole"), "EMPLOYER", StringComparison.Ordinal))
        {
            return RedirectToPage("/Index");
        }

        return null;
    }
}
