using JobPortal.Web.Dtos;
using JobPortal.Web.Dtos.Auth;
using JobPortal.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JobPortal.Web.Pages.Employer;

public class ApplicantsModel : PageModel
{
    private readonly ApiService _api;

    public ApplicantsModel(ApiService api) => _api = api;

    public List<EmployerDashboardApplicationDto> Applicants { get; set; } = new();
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var redirect = RequireEmployer();
        if (redirect != null)
        {
            return redirect;
        }

        var list = await _api.GetApiDataAsync<List<EmployerDashboardApplicationDto>>("/api/employers/me/applications");
        if (list == null)
        {
            ErrorMessage = "Không tải được danh sách ứng viên.";
            return Page();
        }

        Applicants = list;
        return Page();
    }

    private IActionResult? RequireEmployer()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken")))
        {
            return RedirectToPage("/Auth/Login", new { returnUrl = Url.Page("/Employer/Applicants") });
        }

        if (!string.Equals(HttpContext.Session.GetString("UserRole"), "EMPLOYER", StringComparison.Ordinal))
        {
            return RedirectToPage("/Index");
        }

        return null;
    }

    public static string FormatRelativeTime(DateTime utc) => global::JobPortal.Web.Pages.IndexModel.FormatRelativeTime(utc);
}
