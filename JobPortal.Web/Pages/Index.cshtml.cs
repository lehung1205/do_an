using JobPortal.Web.Dtos;
using JobPortal.Web.Dtos.Common;
using JobPortal.Web.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JobPortal.Web.Pages;

public class IndexModel : PageModel
{
    private readonly ApiService _api;
    public List<CongViecDto> Jobs { get; set; } = new();

    public IndexModel(ApiService api) => _api = api;

    public async Task OnGetAsync()
    {
        var paged = await _api.GetApiDataAsync<PagedResult<JobDto>>("/api/jobs?page=1&pageSize=20");
        Jobs = paged?.Items.Select(j => j.ToCongViecDto()).ToList() ?? new();
    }
}
