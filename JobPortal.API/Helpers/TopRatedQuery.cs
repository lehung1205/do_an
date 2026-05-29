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
        var ratings = await context.Reviews
            .AsNoTracking()
            .Where(r => r.ReviewType == ReviewCatalog.SeekerToEmployer)
            .GroupBy(r => r.EmployerId)
            .Select(g => new
            {
                EmployerId = g.Key,
                AverageRating = g.Average(r => r.Rating),
                ReviewCount = g.Count()
            })
            .Where(x => x.ReviewCount > 0)
            .OrderByDescending(x => x.AverageRating)
            .ThenByDescending(x => x.ReviewCount)
            .Take(limit)
            .ToListAsync(cancellationToken);

        if (ratings.Count == 0)
        {
            return Array.Empty<TopRatedUserDto>();
        }

        var ids = ratings.Select(r => r.EmployerId).ToList();
        var employers = await context.Employers
            .AsNoTracking()
            .Where(e => ids.Contains(e.Id))
            .Select(e => new { e.Id, e.Name, e.Email })
            .ToDictionaryAsync(e => e.Id, cancellationToken);

        return ratings.Select(r =>
        {
            employers.TryGetValue(r.EmployerId, out var emp);
            return new TopRatedUserDto
            {
                Id = r.EmployerId,
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
        var ratings = await context.Reviews
            .AsNoTracking()
            .Where(r => r.ReviewType == ReviewCatalog.EmployerToSeeker)
            .GroupBy(r => r.JobSeekerId)
            .Select(g => new
            {
                JobSeekerId = g.Key,
                AverageRating = g.Average(r => r.Rating),
                ReviewCount = g.Count()
            })
            .Where(x => x.ReviewCount > 0)
            .OrderByDescending(x => x.AverageRating)
            .ThenByDescending(x => x.ReviewCount)
            .Take(limit)
            .ToListAsync(cancellationToken);

        if (ratings.Count == 0)
        {
            return Array.Empty<TopRatedUserDto>();
        }

        var ids = ratings.Select(r => r.JobSeekerId).ToList();
        var seekers = await context.JobSeekers
            .AsNoTracking()
            .Where(s => ids.Contains(s.Id))
            .Select(s => new { s.Id, s.Name, s.Email })
            .ToDictionaryAsync(s => s.Id, cancellationToken);

        return ratings.Select(r =>
        {
            seekers.TryGetValue(r.JobSeekerId, out var seeker);
            return new TopRatedUserDto
            {
                Id = r.JobSeekerId,
                Name = seeker?.Name ?? "—",
                Email = seeker?.Email,
                AverageRating = Math.Round(r.AverageRating, 1),
                ReviewCount = r.ReviewCount
            };
        }).ToList();
    }
}
