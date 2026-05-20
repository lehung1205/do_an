using JobPortal.Web.Dtos;
using JobPortal.Web.Dtos.Common;
using JobPortal.Web.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JobPortal.Web.Pages;

public class IndexModel : PageModel
{
    private readonly ApiService _api;

    public List<JobDto> FeaturedJobs { get; set; } = new();
    public int TotalJobCount { get; set; }
    public int EmployerCount { get; set; }
    public int JobSeekerCount { get; set; }
    public string? Q { get; set; }
    public string? Location { get; set; }
    public bool IsEmployerHome { get; set; }
    public EmployerDashboardDto? EmployerDashboard { get; set; }

    public IndexModel(ApiService api) => _api = api;

    public async Task OnGetAsync(string? q, string? location)
    {
        var role = HttpContext.Session.GetString("UserRole");
        var isLoggedIn = !string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken"));

        if (isLoggedIn && string.Equals(role, "EMPLOYER", StringComparison.Ordinal))
        {
            EmployerDashboard = await _api.GetApiDataAsync<EmployerDashboardDto>("/api/employers/me/dashboard");
            IsEmployerHome = EmployerDashboard != null;
            if (IsEmployerHome)
            {
                return;
            }
        }

        Q = q;
        Location = location;

        var pagedTask = _api.GetApiDataAsync<PagedResult<JobDto>>("/api/jobs?page=1&pageSize=50");
        var statsTask = _api.GetApiDataAsync<HomeStatsDto>("/api/stats");
        await Task.WhenAll(pagedTask, statsTask);

        var paged = await pagedTask;
        var stats = await statsTask;
        var jobs = paged?.Items.ToList() ?? new();
        TotalJobCount = paged?.TotalCount ?? jobs.Count;
        EmployerCount = stats?.EmployerCount ?? 0;
        JobSeekerCount = stats?.JobSeekerCount ?? 0;

        if (!string.IsNullOrWhiteSpace(q))
        {
            jobs = jobs
                .Where(j =>
                    j.Title.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    j.Description.Contains(q, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        if (!string.IsNullOrWhiteSpace(location))
        {
            jobs = jobs
                .Where(j => j.Location.Contains(location, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        FeaturedJobs = jobs.Take(6).ToList();
    }

    public static string FormatSalary(int salary) =>
        salary >= 1_000_000
            ? $"{salary / 1_000_000} triệu VNĐ"
            : $"{salary:N0} VNĐ";

    public static string FormatStatus(string status) => status switch
    {
        "recruiting" => "Đang tuyển",
        _ => status
    };

    public static string FormatJobStatus(string status) => status.Trim().ToLowerInvariant() switch
    {
        "recruiting" => "Đang tuyển",
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
