using JobPortal.Web.Dtos;
using JobPortal.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JobPortal.Web.Pages.Applications;

public class IndexModel : PageModel
{
    private readonly ApiService _api;

    public IndexModel(ApiService api)
    {
        _api = api;
    }

    public IReadOnlyList<MyApplicationDto> Applications { get; set; } = Array.Empty<MyApplicationDto>();

    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken")))
        {
            return RedirectToPage("/Auth/Login", new { returnUrl = Url.Page("/Applications/Index") });
        }

        if (!string.Equals(HttpContext.Session.GetString("UserRole"), "JOB_SEEKER", StringComparison.Ordinal))
        {
            return RedirectToPage("/Index");
        }

        var items = await _api.GetApiDataAsync<List<MyApplicationDto>>("/api/applications/me");
        if (items == null)
        {
            ErrorMessage = "Không tải được danh sách đơn ứng tuyển.";
            return Page();
        }

        Applications = items;
        return Page();
    }

    public static string FormatStatus(string status) => status.ToLowerInvariant() switch
    {
        "submitted" => "Đã gửi",
        "pending" => "Chờ xử lý",
        "reviewed" => "Nhà tuyển dụng đã xem",
        "accepted" => "Được chấp nhận",
        "rejected" => "Bị từ chối",
        _ => status
    };

    public static string StatusBadgeClass(string status) => status.ToLowerInvariant() switch
    {
        "submitted" or "pending" => "bg-secondary",
        "reviewed" => "bg-info text-dark",
        "accepted" => "bg-success",
        "rejected" => "bg-danger",
        _ => "bg-secondary"
    };
}
