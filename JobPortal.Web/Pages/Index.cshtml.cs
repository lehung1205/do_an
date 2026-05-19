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
    public string? Q { get; set; }
    public string? Location { get; set; }

    public IndexModel(ApiService api) => _api = api;

    public async Task OnGetAsync(string? q, string? location)
    {
        Q = q;
        Location = location;

        var paged = await _api.GetApiDataAsync<PagedResult<JobDto>>("/api/jobs?page=1&pageSize=50");
        var jobs = paged?.Items.ToList() ?? new();
        TotalJobCount = paged?.TotalCount ?? jobs.Count;

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
