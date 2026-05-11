using JobPortal.Web.Dtos;
using JobPortal.Web.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JobPortal.Web.Pages.Jobs;

public class IndexModel : PageModel
{
    private readonly ApiService _api;
    public List<CongViecDto> Jobs { get; set; } = new();

    public IndexModel(ApiService api) => _api = api;

    public async Task OnGetAsync()
    {
        Jobs = await _api.GetAsync<List<CongViecDto>>("/api/jobs") ?? new();
    }
}
