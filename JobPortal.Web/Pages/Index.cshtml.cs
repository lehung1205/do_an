using JobPortal.Web.Dtos;
using JobPortal.Web.Dtos.Common;
using JobPortal.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JobPortal.Web.Pages;

public class IndexModel : PageModel
{
    private readonly ApiService _api;

    public List<JobDto> FeaturedJobs { get; set; } = new();
    public List<CategoryDto> Categories { get; set; } = new();
    public int TotalJobCount { get; set; }
    public int EmployerCount { get; set; }
    public int JobSeekerCount { get; set; }
    public string? Q { get; set; }
    public string? Location { get; set; }
    public bool HasHomeSearch =>
        !string.IsNullOrWhiteSpace(Q) || !string.IsNullOrWhiteSpace(Location);
    public bool IsEmployerHome { get; set; }
    public EmployerDashboardDto? EmployerDashboard { get; set; }

    public IndexModel(ApiService api) => _api = api;

    public async Task<IActionResult> OnGetAsync(string? q, string? location)
    {
        var role = HttpContext.Session.GetString("UserRole");
        var isLoggedIn = !string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken"));

        if (isLoggedIn && string.Equals(role, "ADMIN", StringComparison.Ordinal))
        {
            return RedirectToPage("/Admin/Dashboard/Index");
        }

        if (isLoggedIn && string.Equals(role, "EMPLOYER", StringComparison.Ordinal))
        {
            EmployerDashboard = await _api.GetApiDataAsync<EmployerDashboardDto>("/api/employers/me/dashboard");
            IsEmployerHome = EmployerDashboard != null;
            if (IsEmployerHome)
            {
                return Page();
            }
        }

        Q = string.IsNullOrWhiteSpace(q) ? null : q.Trim();
        Location = string.IsNullOrWhiteSpace(location) ? null : location.Trim();

        var featuredQuery = new List<string> { "page=1", "pageSize=6" };
        if (!string.IsNullOrEmpty(Q))
        {
            featuredQuery.Add($"q={Uri.EscapeDataString(Q)}");
        }

        if (!string.IsNullOrEmpty(Location))
        {
            featuredQuery.Add($"location={Uri.EscapeDataString(Location)}");
        }

        var featuredTask = _api.GetApiDataAsync<PagedResult<JobDto>>(
            $"/api/jobs?{string.Join("&", featuredQuery)}");
        var totalJobsTask = _api.GetApiDataAsync<PagedResult<JobDto>>("/api/jobs?page=1&pageSize=1");
        var statsTask = _api.GetApiDataAsync<HomeStatsDto>("/api/stats");
        var categoriesTask = _api.GetApiDataAsync<List<CategoryDto>>("/api/categories");
        await Task.WhenAll(featuredTask, totalJobsTask, statsTask, categoriesTask);

        var featured = await featuredTask;
        var totalJobs = await totalJobsTask;
        var stats = await statsTask;
        FeaturedJobs = featured?.Items.ToList() ?? new();
        TotalJobCount = totalJobs?.TotalCount ?? featured?.TotalCount ?? 0;
        EmployerCount = stats?.EmployerCount ?? 0;
        JobSeekerCount = stats?.JobSeekerCount ?? 0;
        Categories = (await categoriesTask ?? new List<CategoryDto>()).Take(8).ToList();
        return Page();
    }

    public static string FormatSalary(string? salary) =>
        string.IsNullOrWhiteSpace(salary) ? "—" : salary.Trim();

    public static string FormatStatus(string status) => status switch
    {
        "recruiting" => "Đang tuyển",
        _ => status
    };

    public static string FormatJobStatus(string status) => status.Trim().ToLowerInvariant() switch
    {
        "pending" => "Chờ duyệt",
        "recruiting" => "Đang tuyển",
        "rejected" => "Từ chối",
        "draft" => "Nháp",
        "closed" => "Đã đóng",
        _ => status
    };

    public static string FormatRelativeTime(DateTime utc)
    {
        var delta = DateTime.UtcNow - utc;
        if (delta.TotalMinutes < 1)
        {
            return "Vừa xong";
        }

        if (delta.TotalHours < 1)
        {
            return $"{(int)delta.TotalMinutes} phút trước";
        }

        if (delta.TotalDays < 1)
        {
            return $"{(int)delta.TotalHours} giờ trước";
        }

        if (delta.TotalDays < 30)
        {
            return $"{(int)delta.TotalDays} ngày trước";
        }

        return utc.ToLocalTime().ToString("dd/MM/yyyy");
    }

    public static string FormatApplicationStatus(string status) => status.Trim().ToLowerInvariant() switch
    {
        "submitted" => "Mới nộp",
        "pending" => "Chờ xử lý",
        "reviewed" => "Đã xem",
        "accepted" => "Chấp nhận",
        "rejected" => "Từ chối",
        _ => status
    };
}
