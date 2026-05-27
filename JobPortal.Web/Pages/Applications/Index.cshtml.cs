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
    public IReadOnlyList<MyApplicationDto> FilteredApplications { get; set; } = Array.Empty<MyApplicationDto>();
    public string StatusFilter { get; set; } = "all";
    public string? Search { get; set; }
    public bool HasActiveFilter => !string.Equals(StatusFilter, "all", StringComparison.OrdinalIgnoreCase) || !string.IsNullOrWhiteSpace(Search);
    public int SubmittedCount { get; set; }
    public int ReviewedCount { get; set; }
    public int AcceptedCount { get; set; }
    public int RejectedCount { get; set; }

    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(string? status, string? q)
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

        Applications = items
            .OrderByDescending(x => x.AppliedAt)
            .ToList();

        SubmittedCount = Applications.Count(x => IsStatus(x.Status, "submitted") || IsStatus(x.Status, "pending"));
        ReviewedCount = Applications.Count(x => IsStatus(x.Status, "reviewed"));
        AcceptedCount = Applications.Count(x => IsStatus(x.Status, "accepted"));
        RejectedCount = Applications.Count(x => IsStatus(x.Status, "rejected"));

        StatusFilter = NormalizeStatus(status);
        Search = string.IsNullOrWhiteSpace(q) ? null : q.Trim();
        FilteredApplications = Applications
            .Where(x => MatchesStatus(x, StatusFilter))
            .Where(x => MatchesSearch(x, Search))
            .ToList();

        return Page();
    }

    private static bool IsStatus(string? value, string expected) =>
        string.Equals(value?.Trim(), expected, StringComparison.OrdinalIgnoreCase);

    private static string NormalizeStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return "all";
        }

        var s = status.Trim().ToLowerInvariant();
        return s is "all" or "submitted" or "reviewed" or "accepted" or "rejected" ? s : "all";
    }

    private static bool MatchesStatus(MyApplicationDto app, string status)
    {
        return status switch
        {
            "submitted" => IsStatus(app.Status, "submitted") || IsStatus(app.Status, "pending"),
            "reviewed" => IsStatus(app.Status, "reviewed"),
            "accepted" => IsStatus(app.Status, "accepted"),
            "rejected" => IsStatus(app.Status, "rejected"),
            _ => true
        };
    }

    private static bool MatchesSearch(MyApplicationDto app, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return true;
        }

        var term = search.Trim();
        return (app.JobTitle?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)
            || (app.JobLocation?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)
            || (app.JobSalary?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)
            || (app.ResumeTitle?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false);
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
