using JobPortal.Web.Dtos;
using JobPortal.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JobPortal.Web.Pages.Employer;

public class ApplicantsModel : PageModel
{
    private readonly ApiService _api;

    public ApplicantsModel(ApiService api) => _api = api;

    public List<EmployerDashboardApplicationDto> Applicants { get; set; } = new();

    public string Filter { get; set; } = "all";

    public int UnreadCount { get; set; }

    public string? ErrorMessage { get; set; }

    public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(string? filter)
    {
        var redirect = RequireEmployer();
        if (redirect != null)
        {
            return redirect;
        }

        Filter = NormalizeFilter(filter);
        SuccessMessage = TempData["ApplicantSuccessMessage"] as string;
        ErrorMessage = TempData["ApplicantErrorMessage"] as string;

        await LoadApplicantsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostUpdateStatusAsync(
        long applicationId,
        string status,
        string? filter)
    {
        var redirect = RequireEmployer();
        if (redirect != null)
        {
            return redirect;
        }

        Filter = NormalizeFilter(filter);

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

        return RedirectToPage(new { filter = Filter });
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

    public async Task<IActionResult> OnGetViewCvAsync(long applicationId, string? filter)
    {
        var redirect = RequireEmployer();
        if (redirect != null)
        {
            return redirect;
        }

        Filter = NormalizeFilter(filter);

        var list = await _api.GetApiDataAsync<List<EmployerDashboardApplicationDto>>("/api/employers/me/applications");
        var app = list?.FirstOrDefault(a => a.Id == applicationId);
        if (app == null)
        {
            TempData["ApplicantErrorMessage"] = "Không tìm thấy đơn ứng tuyển.";
            return RedirectToPage(new { filter = Filter });
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
        return RedirectToPage(new { filter = Filter });
    }

    private async Task LoadApplicantsAsync()
    {
        var list = await _api.GetApiDataAsync<List<EmployerDashboardApplicationDto>>("/api/employers/me/applications");
        if (list == null)
        {
            ErrorMessage ??= "Không tải được danh sách ứng viên.";
            return;
        }

        UnreadCount = list.Count(a => a.IsUnread);

        Applicants = Filter switch
        {
            "unread" => list.Where(a => a.IsUnread).ToList(),
            "reviewed" => list.Where(a => string.Equals(a.Status, "reviewed", StringComparison.OrdinalIgnoreCase)).ToList(),
            "accepted" => list.Where(a => string.Equals(a.Status, "accepted", StringComparison.OrdinalIgnoreCase)).ToList(),
            "rejected" => list.Where(a => string.Equals(a.Status, "rejected", StringComparison.OrdinalIgnoreCase)).ToList(),
            _ => list
        };
    }

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
        "unread" or "reviewed" or "accepted" or "rejected" => filter.Trim().ToLowerInvariant(),
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

    public static string StatusBadgeClass(string status) => status.ToLowerInvariant() switch
    {
        "submitted" or "pending" => "bg-warning text-dark",
        "reviewed" => "bg-info text-dark",
        "accepted" => "bg-success",
        "rejected" => "bg-danger",
        _ => "bg-secondary"
    };
}
