using JobPortal.Web.Dtos;
using JobPortal.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JobPortal.Web.Pages.Employer;

public class JobsModel : PageModel
{
    private readonly ApiService _api;

    public JobsModel(ApiService api) => _api = api;

    public List<EmployerDashboardJobDto> Jobs { get; set; } = new();
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var redirect = RequireEmployer();
        if (redirect != null)
        {
            return redirect;
        }

        var jobs = await _api.GetApiDataAsync<List<EmployerDashboardJobDto>>("/api/employers/me/jobs");
        if (jobs == null)
        {
            ErrorMessage = "Không tải được danh sách tin tuyển dụng.";
            return Page();
        }

        Jobs = jobs;
        return Page();
    }

    private IActionResult? RequireEmployer()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken")))
        {
            return RedirectToPage("/Auth/Login", new { returnUrl = Url.Page("/Employer/Jobs") });
        }

        if (!string.Equals(HttpContext.Session.GetString("UserRole"), "EMPLOYER", StringComparison.Ordinal))
        {
            return RedirectToPage("/Index");
        }

        return null;
    }

    public static string FormatJobStatus(string status) => global::JobPortal.Web.Pages.IndexModel.FormatJobStatus(status);

    public static string FormatSalary(int salary) => global::JobPortal.Web.Pages.IndexModel.FormatSalary(salary);

    public static string FormatRelativeTime(DateTime utc) => global::JobPortal.Web.Pages.IndexModel.FormatRelativeTime(utc);
}
