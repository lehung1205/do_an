using JobPortal.Web.Dtos;
using JobPortal.Web.Dtos.Common;
using JobPortal.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JobPortal.Web.Pages.Admin.Users;

public class IndexModel : PageModel
{
    public const int DefaultPageSize = 15;
    public const string EmployersTab = "employers";
    public const string JobSeekersTab = "seekers";

    private readonly ApiService _api;

    public IndexModel(ApiService api) => _api = api;

    [BindProperty(SupportsGet = true)]
    public string Tab { get; set; } = EmployersTab;

    public List<AdminManagedEmployerDto> Employers { get; set; } = new();
    public List<AdminManagedJobSeekerDto> JobSeekers { get; set; } = new();

    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }
    public string? ActionErrorMessage { get; set; }
    public string? Search { get; set; }
    public string? StatusFilter { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = DefaultPageSize;
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool ShowPagination => TotalPages > 1;
    public bool IsEmployersTab => string.Equals(Tab, EmployersTab, StringComparison.OrdinalIgnoreCase);

    public async Task<IActionResult> OnGetAsync(
        string? tab,
        string? q,
        string? status,
        int page = 1,
        int pageSize = DefaultPageSize)
    {
        var redirect = RequireAdmin();
        if (redirect != null)
        {
            return redirect;
        }

        Tab = string.Equals(tab, JobSeekersTab, StringComparison.OrdinalIgnoreCase)
            ? JobSeekersTab
            : EmployersTab;

        if (page < 1)
        {
            page = 1;
        }

        if (pageSize < 1)
        {
            pageSize = DefaultPageSize;
        }

        Search = string.IsNullOrWhiteSpace(q) ? null : q.Trim();
        StatusFilter = NormalizeStatusFilter(status);
        SuccessMessage = TempData["AdminUserSuccessMessage"] as string;
        ActionErrorMessage = TempData["AdminUserErrorMessage"] as string;

        var query = BuildQuery(page, pageSize);

        if (IsEmployersTab)
        {
            var paged = await _api.GetApiDataAsync<PagedResult<AdminManagedEmployerDto>>(
                $"/api/admin/users/employers?{query}");
            if (paged == null)
            {
                ErrorMessage = "Không tải được danh sách nhà tuyển dụng.";
                return Page();
            }

            Employers = paged.Items.ToList();
            ApplyPaging(paged, page, pageSize);
        }
        else
        {
            var paged = await _api.GetApiDataAsync<PagedResult<AdminManagedJobSeekerDto>>(
                $"/api/admin/users/job-seekers?{query}");
            if (paged == null)
            {
                ErrorMessage = "Không tải được danh sách ứng viên.";
                return Page();
            }

            JobSeekers = paged.Items.ToList();
            ApplyPaging(paged, page, pageSize);
        }

        return Page();
    }

    public Task<IActionResult> OnPostDeactivateEmployerAsync(
        long id, string? tab, string? q, string? status, int page = 1, int pageSize = DefaultPageSize) =>
        SetEmployerStatusAsync(id, active: false, tab, q, status, page, pageSize);

    public Task<IActionResult> OnPostActivateEmployerAsync(
        long id, string? tab, string? q, string? status, int page = 1, int pageSize = DefaultPageSize) =>
        SetEmployerStatusAsync(id, active: true, tab, q, status, page, pageSize);

    public Task<IActionResult> OnPostDeactivateSeekerAsync(
        long id, string? tab, string? q, string? status, int page = 1, int pageSize = DefaultPageSize) =>
        SetJobSeekerStatusAsync(id, active: false, tab, q, status, page, pageSize);

    public Task<IActionResult> OnPostActivateSeekerAsync(
        long id, string? tab, string? q, string? status, int page = 1, int pageSize = DefaultPageSize) =>
        SetJobSeekerStatusAsync(id, active: true, tab, q, status, page, pageSize);

    private async Task<IActionResult> SetEmployerStatusAsync(
        long id,
        bool active,
        string? tab,
        string? q,
        string? status,
        int page,
        int pageSize)
    {
        var redirect = RequireAdmin();
        if (redirect != null)
        {
            return redirect;
        }

        var response = await _api.PostApiResponseAsync<SetAccountActiveRequest, AdminManagedEmployerDto>(
            $"/api/admin/users/employers/{id}/status",
            new SetAccountActiveRequest { Active = active });

        if (response is not { Success: true })
        {
            TempData["AdminUserErrorMessage"] = response?.Message
                ?? response?.Errors.FirstOrDefault()?.Message
                ?? "Không thể cập nhật trạng thái tài khoản.";
        }
        else
        {
            TempData["AdminUserSuccessMessage"] = response.Message
                ?? (active ? "Đã kích hoạt tài khoản." : "Đã vô hiệu hóa tài khoản.");
        }

        return RedirectToPage(new
        {
            tab = EmployersTab,
            q = string.IsNullOrWhiteSpace(q) ? null : q.Trim(),
            status = NormalizeStatusFilter(status) ?? "all",
            page,
            pageSize
        });
    }

    private async Task<IActionResult> SetJobSeekerStatusAsync(
        long id,
        bool active,
        string? tab,
        string? q,
        string? status,
        int page,
        int pageSize)
    {
        var redirect = RequireAdmin();
        if (redirect != null)
        {
            return redirect;
        }

        var response = await _api.PostApiResponseAsync<SetAccountActiveRequest, AdminManagedJobSeekerDto>(
            $"/api/admin/users/job-seekers/{id}/status",
            new SetAccountActiveRequest { Active = active });

        if (response is not { Success: true })
        {
            TempData["AdminUserErrorMessage"] = response?.Message
                ?? response?.Errors.FirstOrDefault()?.Message
                ?? "Không thể cập nhật trạng thái tài khoản.";
        }
        else
        {
            TempData["AdminUserSuccessMessage"] = response.Message
                ?? (active ? "Đã kích hoạt tài khoản." : "Đã vô hiệu hóa tài khoản.");
        }

        return RedirectToPage(new
        {
            tab = JobSeekersTab,
            q = string.IsNullOrWhiteSpace(q) ? null : q.Trim(),
            status = NormalizeStatusFilter(status) ?? "all",
            page,
            pageSize
        });
    }

    private string BuildQuery(int page, int pageSize)
    {
        var parts = new List<string>
        {
            $"page={page}",
            $"pageSize={pageSize}"
        };

        if (!string.IsNullOrEmpty(Search))
        {
            parts.Add($"q={Uri.EscapeDataString(Search)}");
        }

        if (!string.IsNullOrEmpty(StatusFilter))
        {
            parts.Add($"status={Uri.EscapeDataString(StatusFilter)}");
        }

        return string.Join("&", parts);
    }

    private void ApplyPaging<T>(PagedResult<T> paged, int page, int pageSize)
    {
        CurrentPage = paged.Page > 0 ? paged.Page : page;
        PageSize = paged.PageSize > 0 ? paged.PageSize : pageSize;
        TotalCount = paged.TotalCount;
        TotalPages = paged.TotalPages > 0
            ? paged.TotalPages
            : TotalCount == 0
                ? 0
                : (int)Math.Ceiling(TotalCount / (double)PageSize);
    }

    private static string? NormalizeStatusFilter(string? status)
    {
        if (string.IsNullOrWhiteSpace(status) || string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var normalized = status.Trim().ToUpperInvariant();
        return normalized is "ACTIVE" or "INACTIVE" ? normalized : null;
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

    public static bool IsActiveStatus(string status) =>
        string.Equals(status, "ACTIVE", StringComparison.OrdinalIgnoreCase);

    public static string FormatStatus(string status) =>
        IsActiveStatus(status) ? "Hoạt động" : "Vô hiệu hóa";

    public static string StatusBadgeClass(string status) =>
        IsActiveStatus(status) ? "bg-success" : "bg-secondary";
}
