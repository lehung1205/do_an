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
    public DateTime ExpiryDate { get; init; }
    public string? ThumbnailUrl { get; init; }
    public int? ApplicantCount { get; init; }

    public static JobGridCardModel FromJobDto(JobDto job) => new()
    {
        Id = job.Id,
        Title = job.Title,
        Description = job.Description,
        Location = job.Location,
        Salary = job.Salary,
        PostingStatus = job.PostingStatus,
        ExpiryDate = job.ExpiryDate,
        ThumbnailUrl = job.ThumbnailUrl
    };

    public static JobGridCardModel FromEmployerDashboardJob(EmployerDashboardJobDto job) => new()
    {
        Id = job.Id,
        Title = job.Title,
        Description = job.Description,
        Location = job.Location,
        Salary = job.Salary,
        PostingStatus = job.PostingStatus,
        ExpiryDate = job.ExpiryDate,
        ThumbnailUrl = job.ThumbnailUrl,
        ApplicantCount = job.ApplicantCount
    };
}
