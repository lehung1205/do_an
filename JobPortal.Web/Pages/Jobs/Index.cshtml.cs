using JobPortal.Web.Dtos;
using JobPortal.Web.Dtos.Common;
using JobPortal.Web.Helpers;
using JobPortal.Web.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JobPortal.Web.Pages.Jobs;

public class IndexModel : PageModel
{
    public const int DefaultPageSize = 12;

    private readonly ApiService _api;

    public List<JobListItemDto> Jobs { get; set; } = new();
    public List<CategoryDto> Categories { get; set; } = new();
    public string? Q { get; set; }
    public string? Location { get; set; }
    public long? CategoryId { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = DefaultPageSize;
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public int EmployerCount { get; set; }
    public bool HasActiveFilter =>
        !string.IsNullOrEmpty(Q) || !string.IsNullOrEmpty(Location) || CategoryId is > 0;
    public bool ShowPagination => TotalPages > 1;

    public static readonly string[] PopularKeywords =
    {
        "Đà Nẵng", "Intern", "TP.HCM", "Hà Nội", "Giao hàng", "Lễ tân", "Bán hàng", "Phục vụ", "Pha chế"
    };

    public IndexModel(ApiService api) => _api = api;

    public static string FormatPostedTime(DateTime createdAtUtc)
    {
        var diff = DateTime.UtcNow - createdAtUtc.ToUniversalTime();
        if (diff.TotalDays >= 7)
        {
            return createdAtUtc.ToLocalTime().ToString("dd/MM/yyyy");
        }

        if (diff.TotalDays >= 1)
        {
            return $"{(int)diff.TotalDays} ngày trước";
        }

        if (diff.TotalHours >= 1)
        {
            return $"{(int)diff.TotalHours} giờ trước";
        }

        return "Vừa đăng";
    }

    public async Task OnGetAsync(
        string? q,
        string? location,
        long? categoryId,
        int pageNumber = 1,
        int pageSize = DefaultPageSize)
    {
        if (pageNumber < 1)
        {
            pageNumber = 1;
        }

        if (pageSize < 1)
        {
            pageSize = DefaultPageSize;
        }

        Q = string.IsNullOrWhiteSpace(q) ? null : q.Trim();
        Location = string.IsNullOrWhiteSpace(location) ? null : location.Trim();
        CategoryId = categoryId is > 0 ? categoryId : null;

        var query = new List<string>
        {
            $"page={pageNumber}",
            $"pageSize={pageSize}"
        };

        if (!string.IsNullOrEmpty(Q))
        {
            query.Add($"q={Uri.EscapeDataString(Q)}");
        }

        if (!string.IsNullOrEmpty(Location))
        {
            query.Add($"location={Uri.EscapeDataString(Location)}");
        }

        if (CategoryId is > 0)
        {
            query.Add($"categoryId={CategoryId.Value}");
        }

        var jobsTask = _api.GetApiDataAsync<PagedResult<JobListItemDto>>($"/api/jobs?{string.Join("&", query)}");
        var categoriesTask = _api.GetApiDataAsync<List<CategoryDto>>("/api/categories");
        var statsTask = _api.GetApiDataAsync<HomeStatsDto>("/api/stats");

        await Task.WhenAll(jobsTask, categoriesTask, statsTask);

        var paged = await jobsTask;
        Jobs = paged?.Items.ToList() ?? new();
        Categories = CategoryDisplayOrder.SortOtherLast(await categoriesTask ?? new List<CategoryDto>());
        EmployerCount = (await statsTask)?.EmployerCount ?? 0;

        if (paged == null)
        {
            return;
        }

        CurrentPage = paged.Page > 0 ? paged.Page : pageNumber;
        PageSize = paged.PageSize > 0 ? paged.PageSize : pageSize;
        TotalCount = paged.TotalCount;
        TotalPages = paged.TotalPages > 0
            ? paged.TotalPages
            : TotalCount == 0
                ? 0
                : (int)Math.Ceiling(TotalCount / (double)PageSize);
    }
}
