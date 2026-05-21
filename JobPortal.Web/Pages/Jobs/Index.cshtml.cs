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
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = DefaultPageSize;
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }

    public IndexModel(ApiService api) => _api = api;

    public async Task OnGetAsync(int pageNumber = 1, int pageSize = DefaultPageSize)
    {
        if (pageNumber < 1)
        {
            pageNumber = 1;
        }

        if (pageSize < 1)
        {
            pageSize = DefaultPageSize;
        }

        var paged = await _api.GetApiDataAsync<PagedResult<JobDto>>($"/api/jobs?page={pageNumber}&pageSize={pageSize}");
        Jobs = paged?.Items.ToList() ?? new();

        if (paged == null)
        {
            return;
        }

        CurrentPage = paged.Page;
        PageSize = paged.PageSize;
        TotalCount = paged.TotalCount;
        TotalPages = paged.TotalPages;
    }
}
