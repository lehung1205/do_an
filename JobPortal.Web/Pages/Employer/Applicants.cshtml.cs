using JobPortal.Web.Dtos;
using JobPortal.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JobPortal.Web.Pages.Employer;

public class ApplicantsModel : PageModel
{
    private readonly ApiService _api;

    public ApplicantsModel(ApiService api) => _api = api;

    public IReadOnlyList<EmployerDashboardApplicationDto> AllApplicants { get; set; } = Array.Empty<EmployerDashboardApplicationDto>();
    public IReadOnlyList<EmployerDashboardApplicationDto> FilteredApplicants { get; set; } = Array.Empty<EmployerDashboardApplicationDto>();

    public string Filter { get; set; } = "all";
    public string? Search { get; set; }
    public bool HasActiveFilter => !string.Equals(Filter, "all", StringComparison.OrdinalIgnoreCase) || !string.IsNullOrWhiteSpace(Search);

    public int PendingResponseCount { get; set; }
    public int UnreadCount { get; set; }
    public int AcceptedCount { get; set; }
    public int RejectedCount { get; set; }

    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(string? filter, string? q)
    {
        var redirect = RequireEmployer();
        if (redirect != null)
        {
            return redirect;
        }

        Filter = NormalizeFilter(filter);
        Search = string.IsNullOrWhiteSpace(q) ? null : q.Trim();
        SuccessMessage = TempData["ApplicantSuccessMessage"] as string;
        ErrorMessage = TempData["ApplicantErrorMessage"] as string;

        await LoadApplicantsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostUpdateStatusAsync(
        long applicationId,
        string status,
        string? filter,
        string? q)
    {
        var redirect = RequireEmployer();
        if (redirect != null)
        {
            return redirect;
        }

        Filter = NormalizeFilter(filter);
        Search = string.IsNullOrWhiteSpace(q) ? null : q.Trim();

        var response = await _api.PutApiResponseAsync<UpdateEmployerApplicationStatusRequest, EmployerDashboardApplicationDto>(
            $"/api/employers/me/applications/{applicationId}/status",
            new UpdateEmployerApplicationStatusRequest { Status = status });

        if (response is not { Success: true })
        {
            TempData["ApplicantErrorMessage"] = response?.Message
                ?? response?.Errors.FirstOrDefault()?.Message
                ?? "Cập nhật trạng thái thất bại.";
        }
        else
        {
            TempData["ApplicantSuccessMessage"] = FormatSuccessMessage(status);
        }

        return RedirectToPage(new { filter = Filter, q = Search });
    }

    public async Task<IActionResult> OnGetApplicantProfileAsync(long applicationId)
    {
        var redirect = RequireEmployer();
        if (redirect != null)
        {
            return redirect;
        }

        if (applicationId <= 0)
        {
            return NotFound();
        }

        var profile = await _api.GetApiDataAsync<ApplicantProfileForEmployerDto>(
            $"/api/employers/me/applications/{applicationId}/applicant-profile");

        if (profile == null)
        {
            return NotFound();
        }

        return new JsonResult(new
        {
            profile.ApplicationId,
            profile.Name,
            profile.ProfileImage,
            profile.Email,
            profile.Phone,
            dateOfBirth = FormatDateOnly(profile.DateOfBirth),
            gender = FormatGender(profile.Gender),
            profile.Description,
            permanentAddress = DashIfEmpty(profile.PermanentAddress),
            temporaryAddress = DashIfEmpty(profile.TemporaryAddress),
            profile.JobTitle,
            appliedAt = FormatRelativeTime(profile.AppliedAt),
            profile.ResumeTitle,
            applicationStatus = FormatStatus(profile.ApplicationStatus),
            reviews = new
            {
                averageRating = profile.Reviews.AverageRating,
                totalCount = profile.Reviews.TotalCount,
                items = profile.Reviews.Items.Select(i => new
                {
                    i.Id,
                    i.ApplicationId,
                    i.Rating,
                    i.Comment,
                    i.EmployerName,
                    i.JobTitle
                })
            }
        });
    }

    public async Task<IActionResult> OnGetViewCvAsync(long applicationId, string? filter, string? q)
    {
        var redirect = RequireEmployer();
        if (redirect != null)
        {
            return redirect;
        }

        Filter = NormalizeFilter(filter);
        Search = string.IsNullOrWhiteSpace(q) ? null : q.Trim();

        var list = await _api.GetApiDataAsync<List<EmployerDashboardApplicationDto>>("/api/employers/me/applications");
        var app = list?.FirstOrDefault(a => a.Id == applicationId);
        if (app == null)
        {
            TempData["ApplicantErrorMessage"] = "Không tìm thấy đơn ứng tuyển.";
            return RedirectToPage(new { filter = Filter, q = Search });
        }

        if (app.IsUnread)
        {
            await _api.PutApiResponseAsync<UpdateEmployerApplicationStatusRequest, EmployerDashboardApplicationDto>(
                $"/api/employers/me/applications/{applicationId}/status",
                new UpdateEmployerApplicationStatusRequest { Status = "reviewed" });
        }

        if (!string.IsNullOrWhiteSpace(app.ResumeUrl))
        {
            return Redirect(app.ResumeUrl);
        }

        TempData["ApplicantErrorMessage"] = "Ứng viên chưa có liên kết CV.";
        return RedirectToPage(new { filter = Filter, q = Search });
    }

    private async Task LoadApplicantsAsync()
    {
        var list = await _api.GetApiDataAsync<List<EmployerDashboardApplicationDto>>("/api/employers/me/applications");
        if (list == null)
        {
            ErrorMessage ??= "Không tải được danh sách ứng viên.";
            return;
        }

        AllApplicants = list
            .OrderByDescending(a => a.AppliedAt)
            .ToList();

        UnreadCount = AllApplicants.Count(a => a.IsUnread);
        PendingResponseCount = AllApplicants.Count(IsPendingResponse);
        AcceptedCount = AllApplicants.Count(a => IsStatus(a.Status, "accepted"));
        RejectedCount = AllApplicants.Count(a => IsStatus(a.Status, "rejected"));

        FilteredApplicants = AllApplicants
            .Where(a => MatchesFilter(a, Filter))
            .Where(a => MatchesSearch(a, Search))
            .ToList();
    }

    private static bool IsPendingResponse(EmployerDashboardApplicationDto app) =>
        !IsStatus(app.Status, "accepted") && !IsStatus(app.Status, "rejected");

    private static bool MatchesFilter(EmployerDashboardApplicationDto app, string filter) => filter switch
    {
        "pending_response" => IsPendingResponse(app),
        "accepted" => IsStatus(app.Status, "accepted"),
        "rejected" => IsStatus(app.Status, "rejected"),
        _ => true
    };

    private static bool MatchesSearch(EmployerDashboardApplicationDto app, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return true;
        }

        var term = search.Trim();
        return (app.ApplicantName?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)
            || (app.JobTitle?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)
            || (app.CategoryName?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)
            || (app.JobLocation?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)
            || (app.JobSalary?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)
            || (app.ResumeTitle?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)
            || (app.ApplicantEmail?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)
            || (app.ApplicantPhone?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false);
    }

    private static bool IsStatus(string? value, string expected) =>
        string.Equals(value?.Trim(), expected, StringComparison.OrdinalIgnoreCase);

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

    private static string NormalizeFilter(string? filter) => filter?.Trim().ToLowerInvariant() switch
    {
        "accepted" => "accepted",
        "rejected" => "rejected",
        "pending_response" or "no_response" or "unread" or "reviewed" or "submitted" or "pending" => "pending_response",
        _ => "all"
    };

    private static string FormatSuccessMessage(string status) => status.ToLowerInvariant() switch
    {
        "reviewed" => "Đã đánh dấu CV đã xem.",
        "accepted" => "Đã chấp nhận ứng viên.",
        "rejected" => "Đã từ chối ứng viên.",
        _ => "Đã cập nhật trạng thái."
    };

    public static string FormatRelativeTime(DateTime utc) =>
        global::JobPortal.Web.Pages.IndexModel.FormatRelativeTime(utc);

    public static string FormatStatus(string status) => status.ToLowerInvariant() switch
    {
        "submitted" => "Mới nộp",
        "pending" => "Chờ xử lý",
        "reviewed" => "Đã xem",
        "accepted" => "Đã chấp nhận",
        "rejected" => "Từ chối",
        _ => status
    };

    public static string FormatGender(byte? gender) =>
        global::JobPortal.Web.Models.AccountPanelViewModel.FormatGender(gender);

    public static string FormatDateOnly(DateOnly? value) =>
        global::JobPortal.Web.Models.AccountPanelViewModel.FormatDateOnly(value);

    public static string DashIfEmpty(string? value) =>
        global::JobPortal.Web.Models.AccountPanelViewModel.DashIfEmpty(value);

    public static string StatusCardModifier(string status) => status.Trim().ToLowerInvariant() switch
    {
        "accepted" => "emp-app-card--accepted",
        "rejected" => "emp-app-card--rejected",
        "reviewed" => "emp-app-card--reviewed",
        "submitted" or "pending" => "emp-app-card--new",
        _ => ""
    };

    public static string StatusHint(string status, bool isUnread) => status.Trim().ToLowerInvariant() switch
    {
        "submitted" or "pending" when isUnread =>
            "CV mới — xem hồ sơ và cập nhật trạng thái để ứng viên biết tiến độ.",
        "reviewed" => "Đã xem CV — chấp nhận hoặc từ chối khi đã đánh giá xong.",
        "accepted" => "Ứng viên đã được chọn — theo dõi tiến độ công việc tại mục Quản lý tiến độ.",
        "rejected" => "Đơn đã đóng — có thể xem lại tin tuyển dụng hoặc liên hệ nếu cần.",
        _ => ""
    };

    public static string FormatJobPostingStatus(string? status) => status?.Trim().ToLowerInvariant() switch
    {
        "open" => "Đang tuyển",
        "closed" => "Đã đóng",
        _ => status ?? "—"
    };
}
