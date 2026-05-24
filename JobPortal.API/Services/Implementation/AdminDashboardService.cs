using JobPortal.API.Data;
using JobPortal.API.DTOs;
using JobPortal.API.Helpers;
using JobPortal.API.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.API.Services.Implementation;

public class AdminDashboardService : IAdminDashboardService
{
    private const int ChartMonths = 6;
    private const int TopRatedLimit = 10;
    private const int ActiveUserDays = 30;

    private readonly AppDbContext _context;

    public AdminDashboardService(AppDbContext context) => _context = context;

    public async Task<AdminDashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var summary = await BuildSummaryAsync(cancellationToken);
        var topEmployers = await GetTopEmployersAsync(cancellationToken);
        var topSeekers = await GetTopJobSeekersAsync(cancellationToken);
        var charts = await BuildChartsAsync(cancellationToken);

        return new AdminDashboardDto
        {
            Summary = summary,
            TopEmployers = topEmployers,
            TopJobSeekers = topSeekers,
            Charts = charts
        };
    }

    private async Task<AdminDashboardSummaryDto> BuildSummaryAsync(CancellationToken cancellationToken)
    {
        var jobCounts = await _context.Jobs
            .AsNoTracking()
            .GroupBy(j => j.PostingStatus)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        int CountFor(string status) =>
            jobCounts.FirstOrDefault(x => string.Equals(x.Status, status, StringComparison.OrdinalIgnoreCase))?.Count ?? 0;

        var since = DateTime.UtcNow.AddDays(-ActiveUserDays);

        var recentSeekerUserIds = _context.Applications
            .AsNoTracking()
            .Where(a => a.AppliedAt >= since)
            .Select(a => a.JobSeeker.UserId);

        var recentEmployerUserIds = _context.Jobs
            .AsNoTracking()
            .Where(j => j.CreatedAt >= since)
            .Select(j => j.Employer.UserId);

        var recentChatUserIds = _context.ChatMessages
            .AsNoTracking()
            .Where(m => m.SentAt >= since)
            .Select(m => m.SenderUserId);

        var activeUserIds = await recentSeekerUserIds
            .Union(recentEmployerUserIds)
            .Union(recentChatUserIds)
            .Distinct()
            .CountAsync(cancellationToken);

        var totalEmployers = await _context.Employers.AsNoTracking().CountAsync(cancellationToken);
        var totalSeekers = await _context.JobSeekers.AsNoTracking().CountAsync(cancellationToken);
        var totalApplications = await _context.Applications.AsNoTracking().CountAsync(cancellationToken);

        return new AdminDashboardSummaryDto
        {
            PendingJobsCount = CountFor(JobPostingCatalog.Pending),
            ApprovedJobsCount = CountFor(JobPostingCatalog.Recruiting),
            RejectedJobsCount = CountFor(JobPostingCatalog.Rejected),
            ClosedJobsCount = CountFor(JobPostingCatalog.Closed),
            ActiveUsersCount = activeUserIds,
            TotalEmployers = totalEmployers,
            TotalJobSeekers = totalSeekers,
            TotalApplications = totalApplications
        };
    }

    private async Task<IReadOnlyList<AdminRatedUserDto>> GetTopEmployersAsync(CancellationToken cancellationToken)
    {
        var ratings = await _context.Reviews
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
            .Take(TopRatedLimit)
            .ToListAsync(cancellationToken);

        if (ratings.Count == 0)
        {
            return Array.Empty<AdminRatedUserDto>();
        }

        var ids = ratings.Select(r => r.EmployerId).ToList();
        var employers = await _context.Employers
            .AsNoTracking()
            .Where(e => ids.Contains(e.Id))
            .Select(e => new { e.Id, e.Name, e.Email })
            .ToDictionaryAsync(e => e.Id, cancellationToken);

        return ratings.Select(r =>
        {
            employers.TryGetValue(r.EmployerId, out var emp);
            return new AdminRatedUserDto
            {
                Id = r.EmployerId,
                Name = emp?.Name ?? "—",
                Email = emp?.Email,
                AverageRating = Math.Round(r.AverageRating, 1),
                ReviewCount = r.ReviewCount
            };
        }).ToList();
    }

    private async Task<IReadOnlyList<AdminRatedUserDto>> GetTopJobSeekersAsync(CancellationToken cancellationToken)
    {
        var ratings = await _context.Reviews
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
            .Take(TopRatedLimit)
            .ToListAsync(cancellationToken);

        if (ratings.Count == 0)
        {
            return Array.Empty<AdminRatedUserDto>();
        }

        var ids = ratings.Select(r => r.JobSeekerId).ToList();
        var seekers = await _context.JobSeekers
            .AsNoTracking()
            .Where(s => ids.Contains(s.Id))
            .Select(s => new { s.Id, s.Name, s.Email })
            .ToDictionaryAsync(s => s.Id, cancellationToken);

        return ratings.Select(r =>
        {
            seekers.TryGetValue(r.JobSeekerId, out var seeker);
            return new AdminRatedUserDto
            {
                Id = r.JobSeekerId,
                Name = seeker?.Name ?? "—",
                Email = seeker?.Email,
                AverageRating = Math.Round(r.AverageRating, 1),
                ReviewCount = r.ReviewCount
            };
        }).ToList();
    }

    private async Task<AdminRecruitmentChartsDto> BuildChartsAsync(CancellationToken cancellationToken)
    {
        var monthStarts = Enumerable.Range(0, ChartMonths)
            .Select(i => new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-(ChartMonths - 1 - i)))
            .ToList();

        var rangeStart = monthStarts[0];
        var rangeEnd = monthStarts[^1].AddMonths(1);

        var jobsInRange = await _context.Jobs
            .AsNoTracking()
            .Where(j => j.CreatedAt >= rangeStart && j.CreatedAt < rangeEnd)
            .Select(j => new { j.CreatedAt, j.PostingStatus, j.CategoryId })
            .ToListAsync(cancellationToken);

        var applicationsInRange = await _context.Applications
            .AsNoTracking()
            .Where(a => a.AppliedAt >= rangeStart && a.AppliedAt < rangeEnd)
            .Select(a => a.AppliedAt)
            .ToListAsync(cancellationToken);

        var recruitmentTrend = monthStarts.Select(m =>
        {
            var next = m.AddMonths(1);
            var count = jobsInRange.Count(j => j.CreatedAt >= m && j.CreatedAt < next);
            return new AdminChartPointDto { Label = FormatMonthLabel(m), Value = count };
        }).ToList();

        var monthlyApplications = monthStarts.Select(m =>
        {
            var next = m.AddMonths(1);
            var count = applicationsInRange.Count(a => a >= m && a < next);
            return new AdminChartPointDto { Label = FormatMonthLabel(m), Value = count };
        }).ToList();

        var statusLabels = new[]
        {
            (JobPostingCatalog.Pending, "Chờ duyệt"),
            (JobPostingCatalog.Recruiting, "Đang tuyển"),
            (JobPostingCatalog.Rejected, "Từ chối"),
            (JobPostingCatalog.Closed, "Đã đóng"),
            (JobPostingCatalog.Draft, "Nháp")
        };

        var allJobs = await _context.Jobs.AsNoTracking().Select(j => j.PostingStatus).ToListAsync(cancellationToken);
        var jobsByStatus = statusLabels
            .Select(s => new AdminChartPointDto
            {
                Label = s.Item2,
                Value = allJobs.Count(j => string.Equals(j, s.Item1, StringComparison.OrdinalIgnoreCase))
            })
            .Where(p => p.Value > 0)
            .ToList();

        var categoryCounts = await _context.Jobs
            .AsNoTracking()
            .GroupBy(j => j.Category.Name)
            .Select(g => new AdminCategorySliceDto { CategoryName = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(8)
            .ToListAsync(cancellationToken);

        return new AdminRecruitmentChartsDto
        {
            RecruitmentTrend = recruitmentTrend,
            MonthlyApplications = monthlyApplications,
            JobsByStatus = jobsByStatus,
            JobsByCategory = categoryCounts
        };
    }

    private static string FormatMonthLabel(DateTime month) => month.ToString("MM/yyyy");
}
