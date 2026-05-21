using JobPortal.Web.Dtos;
using JobPortal.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JobPortal.Web.Pages.Employer.WorkProgress;

public class DetailModel : PageModel
{
    private readonly ApiService _api;

    public DetailModel(ApiService api) => _api = api;

    [BindProperty(SupportsGet = true)]
    public long ApplicationId { get; set; }

    public ApplicationWorkProgressDto? Progress { get; set; }

    [BindProperty]
    public AddStepInput Input { get; set; } = new();

    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var redirect = RequireEmployer();
        if (redirect != null)
        {
            return redirect;
        }

        if (ApplicationId <= 0)
        {
            return RedirectToPage("/Employer/WorkProgress/Index");
        }

        SuccessMessage = TempData["WorkProgressSuccessMessage"] as string;
        await LoadProgressAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAddStepAsync()
    {
        var redirect = RequireEmployer();
        if (redirect != null)
        {
            return redirect;
        }

        if (ApplicationId <= 0)
        {
            return RedirectToPage("/Employer/WorkProgress/Index");
        }

        await LoadProgressAsync();

        if (Progress?.IsProgressLocked == true)
        {
            ErrorMessage = "Tiến độ đã kết thúc (hoàn thành hoặc đã hủy), không thể cập nhật thêm.";
            return Page();
        }

        ModelState.Clear();

        if (string.IsNullOrWhiteSpace(Input.Status))
        {
            ErrorMessage = "Vui lòng chọn trạng thái công việc.";
            await LoadProgressAsync();
            return Page();
        }

        var response = await _api.PostApiResponseAsync<CreateWorkProgressStepRequest, WorkProgressStepDto>(
            $"/api/employers/me/applications/{ApplicationId}/work-progress",
            new CreateWorkProgressStepRequest
            {
                Status = Input.Status.Trim(),
                Notes = string.IsNullOrWhiteSpace(Input.Notes) ? null : Input.Notes.Trim()
            });

        if (response is not { Success: true })
        {
            ErrorMessage = response?.Message
                ?? response?.Errors.FirstOrDefault()?.Message
                ?? "Không thêm được bước tiến độ.";
            await LoadProgressAsync();
            return Page();
        }

        TempData["WorkProgressSuccessMessage"] = "Đã cập nhật tiến độ làm việc.";
        return RedirectToPage(new { applicationId = ApplicationId });
    }

    private async Task LoadProgressAsync()
    {
        Progress = await _api.GetApiDataAsync<ApplicationWorkProgressDto>(
            $"/api/employers/me/applications/{ApplicationId}/work-progress");

        if (Progress == null)
        {
            ErrorMessage ??= "Không tải được tiến độ làm việc.";
        }
    }

    public static string FormatRelativeTime(DateTime utc) =>
        global::JobPortal.Web.Pages.IndexModel.FormatRelativeTime(utc);

    private IActionResult? RequireEmployer()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken")))
        {
            return RedirectToPage("/Auth/Login", new { returnUrl = Url.Page("/Employer/WorkProgress/Detail", new { applicationId = ApplicationId }) });
        }

        if (!string.Equals(HttpContext.Session.GetString("UserRole"), "EMPLOYER", StringComparison.Ordinal))
        {
            return RedirectToPage("/Index");
        }

        return null;
    }

    public class AddStepInput
    {
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}
