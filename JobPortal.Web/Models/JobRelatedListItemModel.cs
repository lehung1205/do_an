using JobPortal.Web.Dtos;

namespace JobPortal.Web.Models;

public class JobRelatedListItemModel
{
    public long Id { get; init; }
    public string Title { get; init; } = "";
    public string EmployerName { get; init; } = "";
    public string Location { get; init; } = "";
    public string Salary { get; init; } = "";
    public string? ThumbnailUrl { get; init; }
    public bool CompactTitle { get; init; }

    public static JobRelatedListItemModel FromJobSummary(JobSummaryDto job, bool compactTitle = false) => new()
    {
        Id = job.Id,
        Title = job.Title,
        EmployerName = job.EmployerName,
        Location = job.Location,
        Salary = job.Salary,
        ThumbnailUrl = job.ThumbnailUrl,
        CompactTitle = compactTitle
    };
}
