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

    public async Task<IActionResult> OnGetExportJobsByCategoryExcelAsync()
    {
        var redirect = RequireAdmin();
        if (redirect != null)
        {
            return redirect;
        }

        var response = await _api.GetRawAsync("/api/admin/jobs/by-category/export-excel");
        if (!response.IsSuccessStatusCode)
        {
            ErrorMessage = "Không thể xuất báo cáo Excel. Vui lòng thử lại.";
            Dashboard = await _api.GetApiDataAsync<AdminDashboardDto>("/api/admin/dashboard");
            return Page();
        }

        var bytes = await response.Content.ReadAsByteArrayAsync();
        var fileName = response.Content.Headers.ContentDisposition?.FileNameStar
            ?? response.Content.Headers.ContentDisposition?.FileName
            ?? "bao-cao-cong-viec-theo-danh-muc.xlsx";

        return File(
            bytes,
            response.Content.Headers.ContentType?.ToString()
                ?? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName.Trim('"'));
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
