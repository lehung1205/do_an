using JobPortal.Web.Dtos;
using JobPortal.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JobPortal.Web.Pages.Applications.WorkProgress;

public class DetailModel : PageModel
{
    private readonly ApiService _api;

    public DetailModel(ApiService api) => _api = api;

    [BindProperty(SupportsGet = true)]
    public long ApplicationId { get; set; }

    public SeekerApplicationWorkProgressDto? Progress { get; set; }
    public ApplicationReviewContextDto? ReviewContext { get; set; }
    public string? ErrorMessage { get; set; }

    [BindProperty]
    public int ReviewRating { get; set; }

    [BindProperty]
    public string? ReviewComment { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var redirect = RequireJobSeeker();
        if (redirect != null)
        {
            return redirect;
        }

        if (ApplicationId <= 0)
        {
            return RedirectToPage("/Applications/WorkProgress/Index");
        }

        ViewData["ReviewSuccessMessage"] = TempData["ReviewSuccessMessage"] as string;
        ViewData["ReviewErrorMessage"] = TempData["ReviewErrorMessage"] as string;

        await LoadProgressAsync();
        await LoadReviewContextAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostSubmitReviewAsync()
    {
        var redirect = RequireJobSeeker();
        if (redirect != null)
        {
            return redirect;
        }

        if (ApplicationId <= 0)
        {
            return RedirectToPage("/Applications/WorkProgress/Index");
        }

        if (ReviewRating < 1 || ReviewRating > 5)
        {
            TempData["ReviewErrorMessage"] = "Vui lòng chọn điểm đánh giá từ 1 đến 5.";
            return RedirectToPage(new { applicationId = ApplicationId });
        }

        var response = await _api.PostApiResponseAsync<CreateApplicationReviewRequest, ApplicationReviewViewDto>(
            $"/api/applications/me/{ApplicationId}/reviews",
            new CreateApplicationReviewRequest
            {
                Rating = ReviewRating,
                Comment = string.IsNullOrWhiteSpace(ReviewComment) ? null : ReviewComment.Trim()
            });

        if (response is not { Success: true })
        {
            TempData["ReviewErrorMessage"] = response?.Message
                ?? response?.Errors.FirstOrDefault()?.Message
                ?? "Không gửi được đánh giá.";
        }
        else
        {
            TempData["ReviewSuccessMessage"] = "Đã gửi đánh giá nhà tuyển dụng.";
        }

        return RedirectToPage(new { applicationId = ApplicationId });
    }

    private async Task LoadProgressAsync()
    {
        Progress = await _api.GetApiDataAsync<SeekerApplicationWorkProgressDto>(
            $"/api/applications/me/{ApplicationId}/work-progress");

        if (Progress == null)
        {
            ErrorMessage = "Không tải được tiến độ làm việc hoặc đơn chưa được chấp nhận.";
        }
    }

    private async Task LoadReviewContextAsync()
    {
        if (ApplicationId <= 0)
        {
            return;
        }

        ReviewContext = await _api.GetApiDataAsync<ApplicationReviewContextDto>(
            $"/api/applications/me/{ApplicationId}/reviews");
    }

    public static string FormatRelativeTime(DateTime utc) =>
        global::JobPortal.Web.Pages.IndexModel.FormatRelativeTime(utc);

    private IActionResult? RequireJobSeeker()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken")))
        {
            return RedirectToPage("/Auth/Login", new
            {
                returnUrl = Url.Page("/Applications/WorkProgress/Detail", new { applicationId = ApplicationId })
            });
        }

        if (!string.Equals(HttpContext.Session.GetString("UserRole"), "JOB_SEEKER", StringComparison.Ordinal))
        {
            return RedirectToPage("/Index");
        }

        return null;
    }
}
