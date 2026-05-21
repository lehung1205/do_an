using JobPortal.Web.Dtos;
using JobPortal.Web.Dtos.Common;
using JobPortal.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JobPortal.Web.Pages.Employer.WorkProgress;

public class IndexModel : PageModel
{
    public const int DefaultPageSize = 9;

    private readonly ApiService _api;

    public IndexModel(ApiService api) => _api = api;

    public List<EmployerAcceptedApplicationDto> Accepted { get; set; } = new();
    public List<WorkProgressJobOptionDto> JobOptions { get; set; } = new();
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = DefaultPageSize;
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public long? JobId { get; set; }
    public string? Search { get; set; }
    public string? ErrorMessage { get; set; }
    public bool HasAnyAccepted { get; set; }
    public bool ShowPagination => TotalPages > 1;

    public async Task<IActionResult> OnGetAsync(
        long? jobId,
        string? q,
        int pageNumber = 1,
        int pageSize = DefaultPageSize)
    {
        var redirect = RequireEmployer();
        if (redirect != null)
        {
            return redirect;
        }

        if (pageNumber < 1)
        {
            pageNumber = 1;
        }

        if (pageSize < 1)
        {
            pageSize = DefaultPageSize;
        }

        JobId = jobId is > 0 ? jobId : null;
        Search = string.IsNullOrWhiteSpace(q) ? null : q.Trim();

        var jobOptionsTask = _api.GetApiDataAsync<List<WorkProgressJobOptionDto>>(
            "/api/employers/me/applications/accepted/job-options");

        var query = new List<string>
        {
            $"pageNumber={pageNumber}",
            $"pageSize={pageSize}"
        };

        if (JobId.HasValue)
        {
            query.Add($"jobId={JobId.Value}");
        }

        if (!string.IsNullOrEmpty(Search))
        {
            query.Add($"q={Uri.EscapeDataString(Search)}");
        }

        var pagedTask = _api.GetApiDataAsync<PagedResult<EmployerAcceptedApplicationDto>>(
            $"/api/employers/me/applications/accepted?{string.Join("&", query)}");

        await Task.WhenAll(jobOptionsTask, pagedTask);

        JobOptions = await jobOptionsTask ?? new();
        HasAnyAccepted = JobOptions.Sum(j => j.AcceptedCount) > 0;

        var paged = await pagedTask;
        if (paged == null)
        {
            ErrorMessage = "Không tải được danh sách tiến độ làm việc.";
            return Page();
        }

        Accepted = paged.Items.ToList();
        CurrentPage = paged.Page > 0 ? paged.Page : pageNumber;
        PageSize = paged.PageSize > 0 ? paged.PageSize : pageSize;
        TotalCount = paged.TotalCount;

        TotalPages = paged.TotalPages > 0
            ? paged.TotalPages
            : TotalCount == 0
                ? 0
                : (int)Math.Ceiling(TotalCount / (double)PageSize);

        if (CurrentPage < 1)
        {
            CurrentPage = 1;
        }

        return Page();
    }

    public static string FormatRelativeTime(DateTime utc) =>
        global::JobPortal.Web.Pages.IndexModel.FormatRelativeTime(utc);

    private IActionResult? RequireEmployer()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken")))
        {
            return RedirectToPage("/Auth/Login", new { returnUrl = Url.Page("/Employer/WorkProgress/Index") });
        }

        if (!string.Equals(HttpContext.Session.GetString("UserRole"), "EMPLOYER", StringComparison.Ordinal))
        {
            return RedirectToPage("/Index");
        }

        return null;
    }
}
