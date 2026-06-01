using JobPortal.API.Data;
using JobPortal.API.DTOs;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.API.Helpers;

public static class TopRatedQuery
{
    public const int DefaultLimit = 10;

    public static async Task<IReadOnlyList<TopRatedUserDto>> GetTopEmployersAsync(
        AppDbContext context,
        int limit = DefaultLimit,
        CancellationToken cancellationToken = default)
    {
        var globalMean = await GetGlobalMeanAsync(
            context,
            ReviewCatalog.SeekerToEmployer,
            cancellationToken);

        var aggregated = await context.Reviews
            .AsNoTracking()
            .Where(r => r.ReviewType == ReviewCatalog.SeekerToEmployer)
            .GroupBy(r => r.EmployerId)
            .Select(g => new
            {
                Id = g.Key,
                AverageRating = g.Average(r => r.Rating),
                ReviewCount = g.Count()
            })
            .Where(x => x.ReviewCount > 0)
            .ToListAsync(cancellationToken);

        var ranked = TopRatedRanking.RankByWeightedScore(
                aggregated.Select(x => new TopRatedRanking.AggregatedRating(
                    x.Id,
                    x.AverageRating,
                    x.ReviewCount)),
                globalMean,
                limit)
            .ToList();
        if (ranked.Count == 0)
        {
            return Array.Empty<TopRatedUserDto>();
        }

        var ids = ranked.Select(r => r.Id).ToList();
        var employers = await context.Employers
            .AsNoTracking()
            .Where(e => ids.Contains(e.Id))
            .Select(e => new { e.Id, e.Name, Email = e.User.Email })
            .ToDictionaryAsync(e => e.Id, cancellationToken);

        return ranked.Select(r =>
        {
            employers.TryGetValue(r.Id, out var emp);
            return new TopRatedUserDto
            {
                Id = r.Id,
                Name = emp?.Name ?? "—",
                Email = emp?.Email,
                AverageRating = Math.Round(r.AverageRating, 1),
                ReviewCount = r.ReviewCount
            };
        }).ToList();
    }

    public static async Task<IReadOnlyList<TopRatedUserDto>> GetTopJobSeekersAsync(
        AppDbContext context,
        int limit = DefaultLimit,
        CancellationToken cancellationToken = default)
    {
        var globalMean = await GetGlobalMeanAsync(
            context,
            ReviewCatalog.EmployerToSeeker,
            cancellationToken);

        var aggregated = await context.Reviews
            .AsNoTracking()
            .Where(r => r.ReviewType == ReviewCatalog.EmployerToSeeker)
            .GroupBy(r => r.JobSeekerId)
            .Select(g => new
            {
                Id = g.Key,
                AverageRating = g.Average(r => r.Rating),
                ReviewCount = g.Count()
            })
            .Where(x => x.ReviewCount > 0)
            .ToListAsync(cancellationToken);

        var ranked = TopRatedRanking.RankByWeightedScore(
                aggregated.Select(x => new TopRatedRanking.AggregatedRating(
                    x.Id,
                    x.AverageRating,
                    x.ReviewCount)),
                globalMean,
                limit)
            .ToList();
        if (ranked.Count == 0)
        {
            return Array.Empty<TopRatedUserDto>();
        }

        var ids = ranked.Select(r => r.Id).ToList();
        var seekers = await context.JobSeekers
            .AsNoTracking()
            .Where(s => ids.Contains(s.Id))
            .Select(s => new { s.Id, s.Name, Email = s.User.Email })
            .ToDictionaryAsync(s => s.Id, cancellationToken);

        return ranked.Select(r =>
        {
            seekers.TryGetValue(r.Id, out var seeker);
            return new TopRatedUserDto
            {
                Id = r.Id,
                Name = seeker?.Name ?? "—",
                Email = seeker?.Email,
                AverageRating = Math.Round(r.AverageRating, 1),
                ReviewCount = r.ReviewCount
            };
        }).ToList();
    }

    private static async Task<double> GetGlobalMeanAsync(
        AppDbContext context,
        string reviewType,
        CancellationToken cancellationToken)
    {
        var mean = await context.Reviews
            .AsNoTracking()
            .Where(r => r.ReviewType == reviewType)
            .AverageAsync(r => (double?)r.Rating, cancellationToken);

        return mean ?? TopRatedRanking.FallbackGlobalMean;
    }
}
