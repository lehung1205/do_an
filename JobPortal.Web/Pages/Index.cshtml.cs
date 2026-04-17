using JobPortal.Web.Dtos;
using JobPortal.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JobPortal.Web.Pages;

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
