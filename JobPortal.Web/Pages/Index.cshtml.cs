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

    public IndexModel(ApiService api) => _api = api;

    public async Task OnGetAsync(string? q, string? location)
    {
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
}
