using JobPortal.API.Helpers;
using JobPortal.API.Models;

namespace JobPortal.API.Helpers;

public static class ApplicationProgressQueryExtensions
{
    public static IQueryable<Application> FilterByLatestProgressStatus(
        this IQueryable<Application> query,
        string? normalizedProgressFilter)
    {
        if (string.IsNullOrEmpty(normalizedProgressFilter))
        {
            return query;
        }

        if (normalizedProgressFilter == "none")
        {
            return query.Where(a => !a.Processes.Any());
        }

        var statuses = WorkProgressCatalog.GetMatchingStatuses(normalizedProgressFilter);
        if (statuses.Count == 0)
        {
            return query;
        }

        return query.Where(a => a.Processes
            .OrderByDescending(p => p.CreatedAt)
            .ThenByDescending(p => p.Id)
            .Take(1)
            .Any(p => statuses.Contains(p.Status.ToLower())));
    }
}
