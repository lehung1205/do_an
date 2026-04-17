using JobPortal.Web.Dtos;
using JobPortal.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JobPortal.Web.Pages.Jobs;

public class DetailModel : PageModel
{
    private readonly ApiService _api;

    public DetailModel(ApiService api)
    {
        _api = api;
    }

    public JobDto Job { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var job = await _api.GetAsync<JobDto>($"/api/jobs/{id}");
        if (job == null)
        {
            return NotFound();
        }

        Job = job;
        return Page();
    }
}
