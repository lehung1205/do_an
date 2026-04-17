using JobPortal.Web.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using JobPortal.Web.Dtos;

public class IndexModel : PageModel
{
    private readonly ApiService _api;
    public List<JobDto> Jobs { get; set; } = new();

    public IndexModel(ApiService api) => _api = api;

    public async Task OnGetAsync()
    {
        Jobs = await _api.GetAsync<List<JobDto>>("/api/jobs") ?? new();
    }
}