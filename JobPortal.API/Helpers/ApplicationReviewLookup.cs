using JobPortal.API.Models;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.API.Helpers;

public static class ApplicationReviewLookup
{
    public static async Task<IReadOnlyDictionary<long, HashSet<string>>> LoadReviewTypesByApplicationIdsAsync(
        DbSet<Review> reviews,
        IReadOnlyCollection<long> applicationIds,
        CancellationToken cancellationToken = default)
    {
        if (applicationIds.Count == 0)
        {
            return new Dictionary<long, HashSet<string>>();
        }

        var rows = await reviews
            .AsNoTracking()
            .Where(r => applicationIds.Contains(r.ApplicationId))
            .Select(r => new { r.ApplicationId, r.ReviewType })
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(r => r.ApplicationId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.ReviewType).ToHashSet(StringComparer.OrdinalIgnoreCase));
    }

    public static bool HasReviewType(
        IReadOnlyDictionary<long, HashSet<string>> lookup,
        long applicationId,
        string reviewType) =>
        lookup.TryGetValue(applicationId, out var types) &&
        types.Contains(reviewType);
}
