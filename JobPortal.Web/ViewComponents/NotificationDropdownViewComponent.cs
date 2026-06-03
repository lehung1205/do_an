using JobPortal.Web.Dtos;
using JobPortal.Web.Helpers;
using JobPortal.Web.Models;
using JobPortal.Web.Services;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;

namespace JobPortal.Web.ViewComponents;

public class NotificationDropdownViewComponent : ViewComponent
{
    private readonly ApiService _api;
    private readonly IAntiforgery _antiforgery;

    public NotificationDropdownViewComponent(ApiService api, IAntiforgery antiforgery)
    {
        _api = api;
        _antiforgery = antiforgery;
    }

    public async Task<IViewComponentResult> InvokeAsync(string variant = "seeker", bool isActive = false)
    {
        var isEmployer = string.Equals(variant, "employer", StringComparison.OrdinalIgnoreCase);
        var expectedRole = isEmployer ? "EMPLOYER" : "JOB_SEEKER";

        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken"))
            || !string.Equals(HttpContext.Session.GetString("UserRole"), expectedRole, StringComparison.Ordinal))
        {
            return Content(string.Empty);
        }

        var summary = await _api.GetApiDataAsync<NotificationUnreadSummaryDto>("/api/notifications/unread-summary");
        var items = await _api.GetApiDataAsync<List<UserNotificationDto>>("/api/notifications?page=1&pageSize=30")
            ?? new List<UserNotificationDto>();

        var tokens = _antiforgery.GetAndStoreTokens(HttpContext);

        string? ResolveActionUrl(UserNotificationDto n) =>
            isEmployer
                ? NotificationDisplayHelper.GetEmployerActionUrl(n, (page, route) => Url.Page(page, route))
                : NotificationDisplayHelper.GetSeekerActionUrl(n, (page, route) => Url.Page(page, route));

        var model = new NotificationDropdownModel
        {
            Variant = isEmployer ? "employer" : "seeker",
            UnreadCount = summary?.UnreadCount ?? 0,
            IsActive = isActive,
            Groups = NotificationDisplayHelper.GroupNotifications(items, ResolveActionUrl),
            AntiForgeryFieldName = tokens.FormFieldName,
            AntiForgeryRequestToken = tokens.RequestToken ?? string.Empty,
            MarkReadUrl = Url.Page("/Notifications/DropdownHandlers", pageHandler: "MarkRead") ?? "/notifications/dropdown-handlers?handler=MarkRead",
            MarkAllReadUrl = Url.Page("/Notifications/DropdownHandlers", pageHandler: "MarkAllRead") ?? "/notifications/dropdown-handlers?handler=MarkAllRead",
            ViewAllUrl = Url.Page("/Notifications/Index") ?? "/Notifications"
        };

        return View(model);
    }
}
