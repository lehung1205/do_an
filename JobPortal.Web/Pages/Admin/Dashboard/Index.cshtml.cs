using JobPortal.Web.Dtos;
using JobPortal.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JobPortal.Web.Pages.Admin.Dashboard;

public class IndexModel : PageModel
{
    private readonly ApiService _api;

    public IndexModel(ApiService api) => _api = api;

    public AdminDashboardDto? Dashboard { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var redirect = RequireAdmin();
        if (redirect != null)
        {
            return redirect;
        }

        Dashboard = await _api.GetApiDataAsync<AdminDashboardDto>("/api/admin/dashboard");
        if (Dashboard == null)
        {
            ErrorMessage = "Không tải được dữ liệu thống kê. Vui lòng đăng nhập lại hoặc kiểm tra API.";
        }

        return Page();
    }

    private IActionResult? RequireAdmin()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken")))
        {
            return RedirectToPage("/Auth/Login", new
            {
                returnUrl = HttpContext.Request.Path
            });
        }

        if (!string.Equals(HttpContext.Session.GetString("UserRole"), "ADMIN", StringComparison.Ordinal))
        {
            return RedirectToPage("/Index");
        }

        return null;
    }

    public static string FormatRating(double rating) => rating.ToString("0.0");
}
