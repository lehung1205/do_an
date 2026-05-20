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

    public IReadOnlyList<ImageDto> JobImages { get; set; } = Array.Empty<ImageDto>();

    /// <summary>Hiện nút ứng tuyển cho ứng viên (và khách); ẩn với employer/admin.</summary>
    public bool ShowApplyButton { get; set; }

    public async Task<IActionResult> OnGetAsync(long id)
    {
        var job = await _api.GetApiDataAsync<JobDto>($"/api/jobs/{id}");
        if (job == null)
        {
            return NotFound();
        }

        Job = job;
        JobImages = await _api.GetApiDataAsync<List<ImageDto>>($"/api/images/job/{id}") ?? new List<ImageDto>();

        var role = HttpContext.Session.GetString("UserRole");
        ShowApplyButton = !string.Equals(role, "EMPLOYER", StringComparison.Ordinal)
            && !string.Equals(role, "ADMIN", StringComparison.Ordinal);

        return Page();
    }
}
