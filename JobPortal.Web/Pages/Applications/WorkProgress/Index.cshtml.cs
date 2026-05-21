using JobPortal.Web.Dtos;
using JobPortal.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JobPortal.Web.Pages.Applications.WorkProgress;

public class IndexModel : PageModel
{
    private readonly ApiService _api;

    public IndexModel(ApiService api) => _api = api;

    public List<SeekerWorkProgressListItemDto> Items { get; set; } = new();
    public string? Search { get; set; }
    public string? ErrorMessage { get; set; }
    public bool HasActiveFilter => !string.IsNullOrEmpty(Search);

    public async Task<IActionResult> OnGetAsync(string? q)
    {
        var redirect = RequireJobSeeker();
        if (redirect != null)
        {
            return redirect;
        }

        Search = string.IsNullOrWhiteSpace(q) ? null : q.Trim();

        var endpoint = string.IsNullOrEmpty(Search)
            ? "/api/applications/me/accepted/work-progress"
            : $"/api/applications/me/accepted/work-progress?q={Uri.EscapeDataString(Search)}";

        var items = await _api.GetApiDataAsync<List<SeekerWorkProgressListItemDto>>(endpoint);
        if (items == null)
        {
            ErrorMessage = "Không tải được danh sách tiến độ làm việc.";
            return Page();
        }

        Items = items;
        return Page();
    }

    public static string FormatRelativeTime(DateTime utc) =>
        global::JobPortal.Web.Pages.IndexModel.FormatRelativeTime(utc);

    private IActionResult? RequireJobSeeker()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken")))
        {
            return RedirectToPage("/Auth/Login", new { returnUrl = Url.Page("/Applications/WorkProgress/Index") });
        }

        if (!string.Equals(HttpContext.Session.GetString("UserRole"), "JOB_SEEKER", StringComparison.Ordinal))
        {
            return RedirectToPage("/Index");
        }

        return null;
    }
}
