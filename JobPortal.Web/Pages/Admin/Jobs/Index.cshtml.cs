using JobPortal.Web.Dtos;
using JobPortal.Web.Dtos.Common;
using JobPortal.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JobPortal.Web.Pages.Admin.Jobs;

public class IndexModel : PageModel
{
    public const int DefaultPageSize = 12;
    public const string StatusPending = "pending";
    public const string StatusApproved = "recruiting";
    public const string StatusRejected = "rejected";
    public const string StatusAll = "all";

    private readonly ApiService _api;

    public IndexModel(ApiService api) => _api = api;

    public List<AdminPendingJobDto> Jobs { get; set; } = new();
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }
    public string? ActionErrorMessage { get; set; }
    public string? Search { get; set; }
    public string StatusFilter { get; set; } = StatusPending;
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = DefaultPageSize;
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool ShowPagination => TotalPages > 1;

    public async Task<IActionResult> OnGetAsync(
        string? status,
        string? q,
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
        SuccessMessage = TempData["AdminJobSuccessMessage"] as string;
        ActionErrorMessage = TempData["AdminJobErrorMessage"] as string;

        var query = $"page={pageNumber}&pageSize={pageSize}&status={Uri.EscapeDataString(StatusFilter)}";
        if (!string.IsNullOrEmpty(Search))
        {
            query += $"&q={Uri.EscapeDataString(Search)}";
        }

        var paged = await _api.GetApiDataAsync<PagedResult<AdminPendingJobDto>>(
            $"/api/admin/jobs?{query}");

        if (paged == null)
        {
            ErrorMessage = "Không tải được danh sách tin tuyển dụng.";
            return Page();
        }

        Jobs = paged.Items.ToList();
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

    public async Task<IActionResult> OnGetExportJobsExcelAsync(string? status, string? q)
    {
        var redirect = RequireAdmin();
        if (redirect != null)
        {
            return redirect;
        }

        StatusFilter = NormalizeStatusFilter(status);
        Search = string.IsNullOrWhiteSpace(q) ? null : q.Trim();

        var query = $"status={Uri.EscapeDataString(StatusFilter)}";
        if (!string.IsNullOrEmpty(Search))
        {
            query += $"&q={Uri.EscapeDataString(Search)}";
        }

        var response = await _api.GetRawAsync($"/api/admin/jobs/export-excel?{query}");
        if (!response.IsSuccessStatusCode)
        {
            TempData["AdminJobErrorMessage"] = "Không thể xuất báo cáo danh sách công việc.";
            return RedirectToPage("/Admin/Jobs/Index", new { status = StatusFilter, q = Search });
        }

        var bytes = await response.Content.ReadAsByteArrayAsync();
        var fileName = response.Content.Headers.ContentDisposition?.FileNameStar
            ?? response.Content.Headers.ContentDisposition?.FileName
            ?? "danh-sach-cong-viec.xlsx";

        return File(
            bytes,
            response.Content.Headers.ContentType?.ToString()
                ?? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName.Trim('"'));
    }

    public async Task<IActionResult> OnPostApproveAsync(
        long jobId,
        string? status,
        string? q,
        int pageNumber = 1,
        int pageSize = DefaultPageSize) =>
        await ModerateAndRedirectAsync(jobId, approve: true, status, q, pageNumber, pageSize);

    public async Task<IActionResult> OnPostRejectAsync(
        long jobId,
        string? status,
        string? q,
        int pageNumber = 1,
        int pageSize = DefaultPageSize) =>
        await ModerateAndRedirectAsync(jobId, approve: false, status, q, pageNumber, pageSize);

    private async Task<IActionResult> ModerateAndRedirectAsync(
        long jobId,
        bool approve,
        string? status,
        string? q,
        int pageNumber,
        int pageSize)
    {
        var redirect = RequireAdmin();
        if (redirect != null)
        {
            return redirect;
        }

        if (approve)
        {
            var response = await _api.PostApiResponseAsync<object, JobDto>(
                $"/api/admin/jobs/{jobId}/approve",
                new { });

            if (response is not { Success: true })
            {
                TempData["AdminJobErrorMessage"] = response?.Message
                    ?? response?.Errors.FirstOrDefault()?.Message
                    ?? "Không thể duyệt tin.";
            }
            else
            {
                var title = response.Data?.Title;
                TempData["AdminJobSuccessMessage"] = string.IsNullOrWhiteSpace(title)
                    ? "Đã duyệt tin tuyển dụng."
                    : $"Đã duyệt tin \"{title}\".";
            }
        }
        else
        {
            var response = await _api.PostApiResponseAsync<RejectJobRequest, JobDto>(
                $"/api/admin/jobs/{jobId}/reject",
                new RejectJobRequest());

            if (response is not { Success: true })
            {
                TempData["AdminJobErrorMessage"] = response?.Message
                    ?? response?.Errors.FirstOrDefault()?.Message
                    ?? "Không thể từ chối tin.";
            }
            else
            {
                var title = response.Data?.Title;
                TempData["AdminJobSuccessMessage"] = string.IsNullOrWhiteSpace(title)
                    ? "Đã từ chối tin tuyển dụng (đã hoàn lượt đăng cho nhà tuyển dụng)."
                    : $"Đã từ chối tin \"{title}\".";
            }
        }

        return RedirectToPage("/Admin/Jobs/Index", new
        {
            status = NormalizeStatusFilter(status),
            q = string.IsNullOrWhiteSpace(q) ? null : q.Trim(),
            pageNumber,
            pageSize
        });
    }

    private static string NormalizeStatusFilter(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return StatusPending;
        }

        var normalized = status.Trim().ToLowerInvariant();
        return normalized is StatusPending or StatusApproved or StatusRejected or StatusAll
            ? normalized
            : StatusPending;
    }

    public static string FormatStatus(string status) => status.Trim().ToLowerInvariant() switch
    {
        StatusPending => "Chờ duyệt",
        StatusApproved => "Đã duyệt",
        StatusRejected => "Từ chối",
        "closed" => "Đã đóng",
        _ => status
    };

    public static string StatusBadgeClass(string status) => status.Trim().ToLowerInvariant() switch
    {
        StatusPending => "bg-warning text-dark",
        StatusApproved => "bg-success",
        StatusRejected => "bg-danger",
        "closed" => "bg-secondary",
        _ => "bg-secondary"
    };

    public string ListSummaryLabel => StatusFilter switch
    {
        StatusPending => "tin chờ duyệt",
        StatusApproved => "tin đã duyệt",
        StatusRejected => "tin bị từ chối",
        StatusAll => "tin (chờ duyệt / đã duyệt / từ chối)",
        _ => "tin"
    };

    public string EmptyListMessage => StatusFilter switch
    {
        StatusPending => "Không có tin nào đang chờ duyệt.",
        StatusApproved => "Chưa có tin nào đã được duyệt.",
        StatusRejected => "Không có tin nào bị từ chối.",
        StatusAll => "Không có tin phù hợp bộ lọc.",
        _ => "Không có tin."
    };

    private IActionResult? RequireAdmin()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken")))
        {
            return RedirectToPage("/Auth/Login", new { returnUrl = HttpContext.Request.Path + HttpContext.Request.QueryString });
        }

        if (!string.Equals(HttpContext.Session.GetString("UserRole"), "ADMIN", StringComparison.Ordinal))
        {
            return RedirectToPage("/Index");
        }

        return null;
    }
}
