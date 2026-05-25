using JobPortal.Web.Dtos;
using JobPortal.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JobPortal.Web.Pages.Jobs;

public class DetailModel : PageModel
{
    private readonly ApiService _api;

    public DetailModel(ApiService api)
    {
        _api = api;
    }

    public JobDto Job { get; set; } = null!;

    public EmployerPublicProfileDto? EmployerProfile { get; set; }

    public IReadOnlyList<ImageDto> JobImages { get; set; } = Array.Empty<ImageDto>();

    public IReadOnlyList<JobDto> SuggestedJobs { get; set; } = Array.Empty<JobDto>();

    public IReadOnlyList<JobDto> SameCompanyJobs { get; set; } = Array.Empty<JobDto>();

    public IReadOnlyList<JobDto> SimilarJobs { get; set; } = Array.Empty<JobDto>();

    /// <summary>Hiện nút ứng tuyển cho ứng viên (và khách); ẩn với employer/admin.</summary>
    public bool ShowApplyButton { get; set; }

    public bool IsLoggedIn { get; set; }

    public bool IsJobSeeker { get; set; }

    public bool HasApplied { get; set; }

    public bool IsRecruiting { get; set; }

    public bool IsClosed { get; set; }

    public bool IsEmployerViewer { get; set; }

    public IReadOnlyList<ResumeDto> Resumes { get; set; } = Array.Empty<ResumeDto>();

    [BindProperty]
    public long SelectedResumeId { get; set; }

    public string? ApplyErrorMessage { get; set; }

    public string? ApplySuccessMessage { get; set; }

    public string ReturnUrl { get; set; } = "/Jobs";

    public async Task<IActionResult> OnGetAsync(long id)
    {
        var job = await _api.GetApiDataAsync<JobDto>($"/api/jobs/{id}");
        if (job == null)
        {
            return NotFound();
        }

        Job = job;
        EmployerProfile = await _api.GetApiDataAsync<EmployerPublicProfileDto>(
            $"/api/employers/{job.EmployerId}/public-profile");
        JobImages = await _api.GetApiDataAsync<List<ImageDto>>($"/api/images/job/{id}") ?? new List<ImageDto>();

        var related = await _api.GetApiDataAsync<JobRelatedListsDto>($"/api/jobs/{id}/related");
        if (related != null)
        {
            SuggestedJobs = related.SuggestedJobs;
            SameCompanyJobs = related.SameCompanyJobs;
            SimilarJobs = related.SimilarJobs;
        }

        var role = HttpContext.Session.GetString("UserRole");
        ShowApplyButton = !string.Equals(role, "EMPLOYER", StringComparison.Ordinal)
            && !string.Equals(role, "ADMIN", StringComparison.Ordinal);

        IsLoggedIn = !string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken"));
        IsJobSeeker = string.Equals(role, "JOB_SEEKER", StringComparison.Ordinal);
        IsEmployerViewer = string.Equals(role, "EMPLOYER", StringComparison.Ordinal);
        IsClosed = string.Equals(job.PostingStatus, "closed", StringComparison.OrdinalIgnoreCase);
        IsRecruiting = string.Equals(job.PostingStatus, "recruiting", StringComparison.OrdinalIgnoreCase);

        ApplyErrorMessage = TempData["ApplyErrorMessage"] as string;
        ApplySuccessMessage = TempData["ApplySuccessMessage"] as string;
        ReturnUrl = HttpContext.Request.Path + HttpContext.Request.QueryString;

        if (IsJobSeeker && IsLoggedIn)
        {
            Resumes = await _api.GetApiDataAsync<List<ResumeDto>>("/api/resumes/me") ?? new List<ResumeDto>();
            HasApplied = await _api.GetApiDataAsync<bool>($"/api/applications/me/job/{id}/applied") == true;
            if (Resumes.Count > 0 && SelectedResumeId <= 0)
            {
                SelectedResumeId = Resumes[0].Id;
            }
        }

        return Page();
    }

    public async Task<IActionResult> OnPostApplyAsync(long id)
    {
        var job = await _api.GetApiDataAsync<JobDto>($"/api/jobs/{id}");
        if (job == null)
        {
            return NotFound();
        }

        Job = job;

        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken")))
        {
            return RedirectToPage("/Auth/Login", new { returnUrl = Url.Page("/Jobs/Detail", new { id }) });
        }

        if (!string.Equals(HttpContext.Session.GetString("UserRole"), "JOB_SEEKER", StringComparison.Ordinal))
        {
            TempData["ApplyErrorMessage"] = "Chỉ tài khoản ứng viên mới có thể ứng tuyển.";
            return RedirectToPage(new { id });
        }

        if (SelectedResumeId <= 0)
        {
            TempData["ApplyErrorMessage"] = "Vui lòng chọn hồ sơ (CV) để ứng tuyển.";
            return RedirectToPage(new { id });
        }

        var response = await _api.PostApiResponseAsync<CreateApplicationRequest, MyApplicationDto>(
            "/api/applications/me",
            new CreateApplicationRequest { JobId = id, ResumeId = SelectedResumeId });

        if (response is not { Success: true })
        {
            TempData["ApplyErrorMessage"] = response?.Message
                ?? response?.Errors.FirstOrDefault()?.Message
                ?? "Gửi đơn ứng tuyển thất bại.";
            return RedirectToPage(new { id });
        }

        TempData["ApplySuccessMessage"] = "Đã gửi đơn ứng tuyển thành công.";
        return RedirectToPage(new { id });
    }

    public static string FormatPostingStatus(string status) =>
        global::JobPortal.Web.Pages.IndexModel.FormatJobStatus(status);

    public static string FormatGender(byte? gender) =>
        global::JobPortal.Web.Models.AccountPanelViewModel.FormatGender(gender);

    public static string StatusBadgeClass(string status) => status.Trim().ToLowerInvariant() switch
    {
        "pending" => "bg-warning text-dark",
        "recruiting" => "bg-success",
        "rejected" => "bg-danger",
        "closed" => "bg-secondary",
        "draft" => "bg-warning text-dark",
        _ => "bg-secondary"
    };
}
