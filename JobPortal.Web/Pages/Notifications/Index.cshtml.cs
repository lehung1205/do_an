using JobPortal.Web.Dtos;
using JobPortal.Web.Models;
using JobPortal.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JobPortal.Web.Pages.Notifications;

public class IndexModel : PageModel
{
    private readonly ApiService _api;

    public IndexModel(ApiService api) => _api = api;

    public IReadOnlyList<UserNotificationDto> Notifications { get; set; } = Array.Empty<UserNotificationDto>();
    public int UnreadCount { get; set; }
    public bool IsEmployer { get; set; }
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var redirect = RequireNotificationUser();
        if (redirect != null)
        {
            return redirect;
        }

        IsEmployer = string.Equals(HttpContext.Session.GetString("UserRole"), "EMPLOYER", StringComparison.Ordinal);

        var summary = await _api.GetApiDataAsync<NotificationUnreadSummaryDto>("/api/notifications/unread-summary");
        UnreadCount = summary?.UnreadCount ?? 0;

        var items = await _api.GetApiDataAsync<List<UserNotificationDto>>("/api/notifications?page=1&pageSize=50");
        if (items == null)
        {
            ErrorMessage = "Không tải được thông báo.";
            return Page();
        }

        Notifications = items;
        return Page();
    }

    public async Task<IActionResult> OnPostMarkAllReadAsync()
    {
        var redirect = RequireNotificationUser();
        if (redirect != null)
        {
            return redirect;
        }

        await _api.PostApiResponseAsync<object, object>("/api/notifications/read-all", new { });
        SuccessMessage = "Đã đánh dấu tất cả là đã đọc.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostMarkReadAsync(long id)
    {
        var redirect = RequireNotificationUser();
        if (redirect != null)
        {
            return redirect;
        }

        await _api.PostApiResponseAsync<object, object>($"/api/notifications/{id}/read", new { });
        return RedirectToPage();
    }

    private IActionResult? RequireNotificationUser()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken")))
        {
            return RedirectToPage("/Auth/Login", new { returnUrl = Url.Page("/Notifications/Index") });
        }

        var role = HttpContext.Session.GetString("UserRole");
        if (!string.Equals(role, "EMPLOYER", StringComparison.Ordinal)
            && !string.Equals(role, "JOB_SEEKER", StringComparison.Ordinal))
        {
            return RedirectToPage("/Index");
        }

        return null;
    }

    public static string FormatRelativeTime(DateTime createdAtUtc)
    {
        var created = createdAtUtc.ToLocalTime();
        var diff = DateTime.Now - created;

        if (diff.TotalMinutes < 1)
        {
            return "Vừa xong";
        }

        if (diff.TotalHours < 1)
        {
            return $"{(int)diff.TotalMinutes} phút trước";
        }

        if (diff.TotalDays < 1)
        {
            return $"{(int)diff.TotalHours} giờ trước";
        }

        if (diff.TotalDays < 7)
        {
            return $"{(int)diff.TotalDays} ngày trước";
        }

        return created.ToString("dd/MM/yyyy HH:mm");
    }

    public string? GetActionUrl(UserNotificationDto notification)
    {
        if (notification.ReferenceId == null || string.IsNullOrWhiteSpace(notification.ReferenceType))
        {
            return null;
        }

        if (IsEmployer && string.Equals(notification.ReferenceType, "job", StringComparison.OrdinalIgnoreCase))
        {
            return Url.Page("/Employer/Jobs");
        }

        if (!IsEmployer && string.Equals(notification.ReferenceType, "application", StringComparison.OrdinalIgnoreCase))
        {
            var status = string.Equals(notification.Type, "application_accepted", StringComparison.OrdinalIgnoreCase)
                ? "accepted"
                : "rejected";
            return Url.Page("/Applications/Index", new { status });
        }

        return null;
    }

    public static string IconClass(string type) => type switch
    {
        "job_approved" or "application_accepted" => "bi-check-circle-fill text-success",
        "job_rejected" or "application_rejected" => "bi-x-circle-fill text-danger",
        _ => "bi-bell-fill text-primary"
    };
}
