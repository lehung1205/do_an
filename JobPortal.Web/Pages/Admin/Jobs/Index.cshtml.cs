using JobPortal.Web.Dtos;
using JobPortal.Web.Dtos.Common;
using JobPortal.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JobPortal.Web.Pages.Admin.Jobs;

public class IndexModel : PageModel
{
    public const int DefaultPageSize = 12;

    private readonly ApiService _api;

    public IndexModel(ApiService api) => _api = api;

    public List<AdminPendingJobDto> Jobs { get; set; } = new();
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }
    public string? ActionErrorMessage { get; set; }
    public string? Search { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = DefaultPageSize;
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool ShowPagination => TotalPages > 1;

    public async Task<IActionResult> OnGetAsync(string? q, int page = 1, int pageSize = DefaultPageSize)
    {
        var redirect = RequireAdmin();
        if (redirect != null)
        {
            return redirect;
        }

        if (page < 1)
        {
            page = 1;
        }

        if (pageSize < 1)
        {
            pageSize = DefaultPageSize;
        }

        Search = string.IsNullOrWhiteSpace(q) ? null : q.Trim();
        SuccessMessage = TempData["AdminJobSuccessMessage"] as string;
        ActionErrorMessage = TempData["AdminJobErrorMessage"] as string;

        var query = $"page={page}&pageSize={pageSize}";
        if (!string.IsNullOrEmpty(Search))
        {
            query += $"&q={Uri.EscapeDataString(Search)}";
        }

        var paged = await _api.GetApiDataAsync<PagedResult<AdminPendingJobDto>>(
            $"/api/admin/jobs/pending?{query}");

        if (paged == null)
        {
            ErrorMessage = "Không tải được danh sách tin chờ duyệt.";
            return Page();
        }

        Jobs = paged.Items.ToList();
        CurrentPage = paged.Page > 0 ? paged.Page : page;
        PageSize = paged.PageSize > 0 ? paged.PageSize : pageSize;
        TotalCount = paged.TotalCount;
        TotalPages = paged.TotalPages > 0
            ? paged.TotalPages
            : TotalCount == 0
                ? 0
                : (int)Math.Ceiling(TotalCount / (double)PageSize);

        return Page();
    }

    public async Task<IActionResult> OnPostApproveAsync(long jobId, string? q, int page = 1, int pageSize = DefaultPageSize)
    {
        var redirect = RequireAdmin();
        if (redirect != null)
        {
            return redirect;
        }

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

        return RedirectToPage(new { q = string.IsNullOrWhiteSpace(q) ? null : q.Trim(), page, pageSize });
    }

    public async Task<IActionResult> OnPostRejectAsync(long jobId, string? q, int page = 1, int pageSize = DefaultPageSize)
    {
        var redirect = RequireAdmin();
        if (redirect != null)
        {
            return redirect;
        }

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

        return RedirectToPage(new { q = string.IsNullOrWhiteSpace(q) ? null : q.Trim(), page, pageSize });
    }

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
