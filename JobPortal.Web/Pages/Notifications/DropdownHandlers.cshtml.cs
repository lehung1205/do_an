using JobPortal.Web.Dtos;
using JobPortal.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JobPortal.Web.Pages.Notifications;

public class DropdownHandlersModel : PageModel
{
    private readonly ApiService _api;

    public DropdownHandlersModel(ApiService api) => _api = api;

    public async Task<IActionResult> OnPostMarkReadAsync(long id)
    {
        if (!CanUseNotifications())
        {
            return new JsonResult(new { success = false, message = "Unauthorized" }) { StatusCode = 401 };
        }

        var response = await _api.PostApiResponseAsync<object, object>($"/api/notifications/{id}/read", new { });
        if (response is not { Success: true })
        {
            return new JsonResult(new { success = false, message = response?.Message ?? "Không thể đánh dấu đã đọc." });
        }

        var summary = await _api.GetApiDataAsync<NotificationUnreadSummaryDto>("/api/notifications/unread-summary");
        return new JsonResult(new { success = true, unreadCount = summary?.UnreadCount ?? 0 });
    }

    public async Task<IActionResult> OnPostMarkAllReadAsync()
    {
        if (!CanUseNotifications())
        {
            return new JsonResult(new { success = false, message = "Unauthorized" }) { StatusCode = 401 };
        }

        var response = await _api.PostApiResponseAsync<object, object>("/api/notifications/read-all", new { });
        if (response is not { Success: true })
        {
            return new JsonResult(new { success = false, message = response?.Message ?? "Không thể đánh dấu tất cả." });
        }

        return new JsonResult(new { success = true, unreadCount = 0 });
    }

    private bool CanUseNotifications()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken")))
        {
            return false;
        }

        var role = HttpContext.Session.GetString("UserRole");
        return string.Equals(role, "JOB_SEEKER", StringComparison.Ordinal)
            || string.Equals(role, "EMPLOYER", StringComparison.Ordinal);
    }
}
