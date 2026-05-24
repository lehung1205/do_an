using JobPortal.Web.Dtos;
using JobPortal.Web.Dtos.Common;
using JobPortal.Web.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JobPortal.Web.Pages.Jobs;

public class IndexModel : PageModel
{
    public const int DefaultPageSize = 12;

    private readonly ApiService _api;

    public List<JobDto> Jobs { get; set; } = new();
    public string? Q { get; set; }
    public string? Location { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = DefaultPageSize;
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasActiveFilter => !string.IsNullOrEmpty(Q) || !string.IsNullOrEmpty(Location);
    public bool ShowPagination => TotalPages > 1;

    public IndexModel(ApiService api) => _api = api;

    public async Task OnGetAsync(
        string? q,
        string? location,
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

        var paged = await _api.GetApiDataAsync<PagedResult<JobDto>>(
            $"/api/jobs?{string.Join("&", query)}");

        Jobs = paged?.Items.ToList() ?? new();

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
