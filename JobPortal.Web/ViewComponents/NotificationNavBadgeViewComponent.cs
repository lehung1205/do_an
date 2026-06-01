using JobPortal.Web.Dtos;
using JobPortal.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace JobPortal.Web.ViewComponents;

public class NotificationNavBadgeViewComponent : ViewComponent
{
    private readonly ApiService _api;

    public NotificationNavBadgeViewComponent(ApiService api) => _api = api;

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var role = HttpContext.Session.GetString("UserRole");
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken"))
            || (!string.Equals(role, "EMPLOYER", StringComparison.Ordinal)
                && !string.Equals(role, "JOB_SEEKER", StringComparison.Ordinal)))
        {
            return Content(string.Empty);
        }

        var summary = await _api.GetApiDataAsync<NotificationUnreadSummaryDto>("/api/notifications/unread-summary");
        var count = summary?.UnreadCount ?? 0;
        return View(count);
    }
}
