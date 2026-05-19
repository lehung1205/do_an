using JobPortal.Web.Dtos;
using JobPortal.Web.Dtos.Common;
using JobPortal.Web.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JobPortal.Web.Pages.Jobs;

public class IndexModel : PageModel
{
    private readonly ApiService _api;
    public List<CongViecDto> Jobs { get; set; } = new();

    public IndexModel(ApiService api) => _api = api;

    public async Task OnGetAsync(int page = 1, int pageSize = 20)
    {
        var paged = await _api.GetApiDataAsync<PagedResult<JobDto>>($"/api/jobs?page={page}&pageSize={pageSize}");
        Jobs = paged?.Items.Select(j => j.ToCongViecDto()).ToList() ?? new();
    }
}
