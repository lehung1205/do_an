using JobPortal.Web.Dtos;
using JobPortal.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace JobPortal.Web.ViewComponents;

public class AdminPortalNavViewComponent : ViewComponent
{
    private readonly ApiService _api;

    public AdminPortalNavViewComponent(ApiService api) => _api = api;

    public async Task<IViewComponentResult> InvokeAsync(string currentPage = "")
    {
        var summary = await _api.GetApiDataAsync<AdminJobModerationSummaryDto>("/api/admin/jobs/summary");

        return View(new AdminPortalNavViewModel
        {
            CurrentPage = currentPage ?? "",
            PendingJobsCount = summary?.PendingCount ?? 0
        });
    }
}

public class AdminPortalNavViewModel
{
    public string CurrentPage { get; init; } = "";

    public int PendingJobsCount { get; init; }

    public bool IsDashboard =>
        string.Equals(CurrentPage, "/Admin/Dashboard/Index", StringComparison.OrdinalIgnoreCase);

    public bool IsJobs =>
        CurrentPage.StartsWith("/Admin/Jobs/", StringComparison.OrdinalIgnoreCase);

    public bool IsUsers =>
        CurrentPage.StartsWith("/Admin/Users/", StringComparison.OrdinalIgnoreCase);

    public bool IsPayments =>
        CurrentPage.StartsWith("/Admin/Payments/", StringComparison.OrdinalIgnoreCase);

    public bool IsPackages =>
        CurrentPage.StartsWith("/Admin/Packages/", StringComparison.OrdinalIgnoreCase);
}
