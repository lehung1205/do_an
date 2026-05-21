using JobPortal.Web.Dtos;
using JobPortal.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JobPortal.Web.Pages.Applications.WorkProgress;

public class DetailModel : PageModel
{
    private readonly ApiService _api;

    public DetailModel(ApiService api) => _api = api;

    [BindProperty(SupportsGet = true)]
    public long ApplicationId { get; set; }

    public SeekerApplicationWorkProgressDto? Progress { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var redirect = RequireJobSeeker();
        if (redirect != null)
        {
            return redirect;
        }

        if (ApplicationId <= 0)
        {
            return RedirectToPage("/Applications/WorkProgress/Index");
        }

        Progress = await _api.GetApiDataAsync<SeekerApplicationWorkProgressDto>(
            $"/api/applications/me/{ApplicationId}/work-progress");

        if (Progress == null)
        {
            ErrorMessage = "Không tải được tiến độ làm việc hoặc đơn chưa được chấp nhận.";
        }

        return Page();
    }

    public static string FormatRelativeTime(DateTime utc) =>
        global::JobPortal.Web.Pages.IndexModel.FormatRelativeTime(utc);

    private IActionResult? RequireJobSeeker()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken")))
        {
            return RedirectToPage("/Auth/Login", new
            {
                returnUrl = Url.Page("/Applications/WorkProgress/Detail", new { applicationId = ApplicationId })
            });
        }

        if (!string.Equals(HttpContext.Session.GetString("UserRole"), "JOB_SEEKER", StringComparison.Ordinal))
        {
            return RedirectToPage("/Index");
        }

        return null;
    }
}
