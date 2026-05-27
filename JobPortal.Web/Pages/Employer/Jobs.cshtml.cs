using JobPortal.Web.Dtos;
using JobPortal.Web.Dtos.Common;
using JobPortal.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JobPortal.Web.Pages.Employer;

public class JobsModel : PageModel
{
    public const int DefaultPageSize = 9;

    private readonly ApiService _api;

    public JobsModel(ApiService api) => _api = api;

    public List<EmployerDashboardJobDto> Jobs { get; set; } = new();
    public EmployerDashboardStatsDto? Stats { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ActionErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }
    public string? StatusFilter { get; set; }
    public string? Search { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = DefaultPageSize;
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasActiveFilter => !string.IsNullOrEmpty(StatusFilter) || !string.IsNullOrEmpty(Search);
    public bool ShowPagination => TotalPages > 1;

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

        StatusFilter = NormalizeStatusFilter(status);
        Search = string.IsNullOrWhiteSpace(q) ? null : q.Trim();
        SuccessMessage = TempData["JobManageSuccessMessage"] as string;
        ActionErrorMessage = TempData["JobManageErrorMessage"] as string;

        var dashboardTask = _api.GetApiDataAsync<EmployerDashboardDto>("/api/employers/me/dashboard");

        var query = new List<string>
        {
            $"pageNumber={pageNumber}",
            $"pageSize={pageSize}"
        };

        if (!string.IsNullOrEmpty(StatusFilter))
        {
            query.Add($"status={Uri.EscapeDataString(StatusFilter)}");
        }

        if (!string.IsNullOrEmpty(Search))
        {
            query.Add($"q={Uri.EscapeDataString(Search)}");
        }

        var jobsTask = _api.GetApiDataAsync<PagedResult<EmployerDashboardJobDto>>(
            $"/api/employers/me/jobs?{string.Join("&", query)}");
        await Task.WhenAll(dashboardTask, jobsTask);

        Stats = (await dashboardTask)?.Stats;
        var paged = await jobsTask;

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

        if (CurrentPage < 1)
        {
            CurrentPage = 1;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostCloseJobAsync(
        long jobId,
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

        var response = await _api.PostApiResponseAsync<object, EmployerDashboardJobDto>(
            $"/api/employers/me/jobs/{jobId}/close",
            new { });

        if (response is not { Success: true })
        {
            TempData["JobManageErrorMessage"] = response?.Message
                ?? response?.Errors.FirstOrDefault()?.Message
                ?? "Không thể đóng tin tuyển dụng.";
        }
        else
        {
            var title = response.Data?.Title;
            TempData["JobManageSuccessMessage"] = string.IsNullOrWhiteSpace(title)
                ? "Đã đóng tin tuyển dụng."
                : $"Đã đóng tin \"{title}\".";
        }

        var statusFilter = NormalizeStatusFilter(status);
        return RedirectToPage(new
        {
            status = string.IsNullOrEmpty(statusFilter) ? "all" : statusFilter,
            q = string.IsNullOrWhiteSpace(q) ? null : q.Trim(),
            pageNumber,
            pageSize
        });
    }

    private static string? NormalizeStatusFilter(string? status)
    {
        if (string.IsNullOrWhiteSpace(status) || string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var normalized = status.Trim().ToLowerInvariant();
        return normalized is "pending" or "recruiting" or "rejected" or "closed"
            ? normalized
            : null;
    }

    private IActionResult? RequireEmployer()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken")))
        {
            return RedirectToPage("/Auth/Login", new { returnUrl = Url.Page("/Employer/Jobs") });
        }

        if (!string.Equals(HttpContext.Session.GetString("UserRole"), "EMPLOYER", StringComparison.Ordinal))
        {
            return RedirectToPage("/Index");
        }

        return null;
    }

    public static string FormatJobStatus(string status) => global::JobPortal.Web.Pages.IndexModel.FormatJobStatus(status);

    public static string FormatSalary(string? salary) => global::JobPortal.Web.Pages.IndexModel.FormatSalary(salary);

    public static string FormatRelativeTime(DateTime utc) => global::JobPortal.Web.Pages.IndexModel.FormatRelativeTime(utc);
}
