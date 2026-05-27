using JobPortal.Web.Dtos;

namespace JobPortal.Web.Models;

public class JobGridCardModel
{
    public long Id { get; init; }
    public string Title { get; init; } = "";
    public string Description { get; init; } = "";
    public string Location { get; init; } = "";
    public string Salary { get; init; } = null!;
    public string PostingStatus { get; init; } = "";
    public string? EmployerName { get; init; }
    public double? EmployerAverageRating { get; init; }
    public int EmployerReviewCount { get; init; }
    public DateTime ExpiryDate { get; init; }
    public string? ThumbnailUrl { get; init; }
    public int? ApplicantCount { get; init; }
    public string? WorkingHours { get; init; }
    public bool ShowEmployerActions { get; init; }
    /// <summary>Hiển thị badge trạng thái theo màu NTD (trang chủ employer, không cần nút thao tác).</summary>
    public bool ShowEmployerPostingStatus { get; init; }
    public string? ListStatusFilter { get; init; }
    public string? ListSearch { get; init; }
    public int ListPageNumber { get; init; } = 1;
    public int ListPageSize { get; init; } = 9;

    public static JobGridCardModel FromJobListItem(JobListItemDto job) => new()
    {
        Id = job.Id,
        Title = job.Title,
        Description = job.DescriptionPreview,
        Location = job.Location,
        Salary = job.Salary,
        PostingStatus = "recruiting",
        EmployerName = job.EmployerName,
        EmployerAverageRating = job.EmployerAverageRating,
        EmployerReviewCount = job.EmployerReviewCount,
        ExpiryDate = job.ExpiryDate,
        ThumbnailUrl = job.ThumbnailUrl,
        WorkingHours = job.WorkingHours
    };

    public static JobGridCardModel FromEmployerDashboardJob(EmployerDashboardJobDto job) => new()
    {
        Id = job.Id,
        Title = job.Title,
        Description = job.Description,
        Location = job.Location,
        Salary = job.Salary,
        PostingStatus = job.PostingStatus,
        EmployerAverageRating = job.EmployerAverageRating,
        EmployerReviewCount = job.EmployerReviewCount,
        ExpiryDate = job.ExpiryDate,
        ThumbnailUrl = job.ThumbnailUrl,
        ApplicantCount = job.ApplicantCount,
        ShowEmployerPostingStatus = true
    };

    public static JobGridCardModel FromEmployerManageJob(
        EmployerDashboardJobDto job,
        string? statusFilter,
        string? search,
        int pageNumber,
        int pageSize) => new()
    {
        Id = job.Id,
        Title = job.Title,
        Description = job.Description,
        Location = job.Location,
        Salary = job.Salary,
        PostingStatus = job.PostingStatus,
        EmployerAverageRating = job.EmployerAverageRating,
        EmployerReviewCount = job.EmployerReviewCount,
        ExpiryDate = job.ExpiryDate,
        ThumbnailUrl = job.ThumbnailUrl,
        ApplicantCount = job.ApplicantCount,
        WorkingHours = job.WorkingHours,
        ShowEmployerActions = true,
        ListStatusFilter = statusFilter,
        ListSearch = search,
        ListPageNumber = pageNumber,
        ListPageSize = pageSize
    };
}
