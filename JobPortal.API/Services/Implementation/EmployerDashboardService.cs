using JobPortal.API.Data;
using JobPortal.API.DTOs;
using JobPortal.API.Exceptions;
using JobPortal.API.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.API.Services.Implementation;

public class EmployerDashboardService : IEmployerDashboardService
{
    private readonly AppDbContext _context;

    public EmployerDashboardService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<EmployerDashboardDto> GetDashboardForUserAsync(long userId, CancellationToken cancellationToken = default)
    {
        var employer = await _context.Employers
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.UserId == userId, cancellationToken);

        if (employer == null)
        {
            throw new NotFoundException("Employer profile not found for this user.");
        }

        var now = DateTime.UtcNow;
        var todayStart = now.Date;
        var weekAgo = now.AddDays(-7);
        var expiringThreshold = now.AddDays(7);

        var jobs = await QueryJobsAsync(employer.Id, cancellationToken);

        var applications = await QueryApplicationsAsync(employer.Id, take: 10, cancellationToken);

        var allApplicationsQuery = _context.Applications.AsNoTracking()
            .Where(a => a.Job.EmployerId == employer.Id);

        var totalCv = await allApplicationsQuery.CountAsync(cancellationToken);
        var newCv = await allApplicationsQuery.CountAsync(a => a.AppliedAt >= weekAgo, cancellationToken);
        var newToday = await allApplicationsQuery.CountAsync(a => a.AppliedAt >= todayStart, cancellationToken);
        var unreadCv = await allApplicationsQuery.CountAsync(
            a => a.Status != "reviewed" && a.Status != "rejected",
            cancellationToken);

        var openJobs = jobs.Count(j => string.Equals(j.PostingStatus, "recruiting", StringComparison.OrdinalIgnoreCase));
        var expiringSoon = jobs.Count(j =>
            string.Equals(j.PostingStatus, "recruiting", StringComparison.OrdinalIgnoreCase) &&
            j.ExpiryDate <= expiringThreshold &&
            j.ExpiryDate >= now);

        var notifications = new List<EmployerDashboardNotificationDto>();

        foreach (var job in jobs.Where(j =>
                     string.Equals(j.PostingStatus, "recruiting", StringComparison.OrdinalIgnoreCase) &&
                     j.ExpiryDate <= now.AddDays(2) &&
                     j.ExpiryDate >= now))
        {
            var days = (int)Math.Ceiling((job.ExpiryDate - now).TotalDays);
            if (days < 1)
            {
                days = 1;
            }

            notifications.Add(new EmployerDashboardNotificationDto
            {
                Type = "warning",
                Message = $"Job \"{job.Title}\" sẽ hết hạn sau {days} ngày"
            });
        }

        if (unreadCv > 0)
        {
            notifications.Add(new EmployerDashboardNotificationDto
            {
                Type = "warning",
                Message = $"Bạn còn {unreadCv} CV chưa xem"
            });
        }

        if (employer.PostingLimit <= 5)
        {
            notifications.Add(new EmployerDashboardNotificationDto
            {
                Type = "info",
                Message = "Nâng cấp gói để đăng không giới hạn"
            });
        }

        return new EmployerDashboardDto
        {
            CompanyName = employer.Name,
            NewApplicantsToday = newToday,
            PostingLimit = employer.PostingLimit,
            Stats = new EmployerDashboardStatsDto
            {
                OpenJobs = openJobs,
                NewCvCount = newCv,
                TotalCvCount = totalCv,
                ExpiringSoonCount = expiringSoon
            },
            RecentJobs = jobs.Take(5).Select(MapJobDto).ToList(),
            RecentApplications = applications.Select(a => new EmployerDashboardApplicationDto
            {
                Id = a.Id,
                ApplicantName = a.ApplicantName,
                JobTitle = a.JobTitle,
                AppliedAt = a.AppliedAt,
                ResumeUrl = a.ResumeUrl,
                Status = a.Status
            }).ToList(),
            Notifications = notifications
        };
    }

    public async Task<IReadOnlyList<EmployerDashboardJobDto>> GetJobsForUserAsync(
        long userId,
        CancellationToken cancellationToken = default)
    {
        var employer = await _context.Employers
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.UserId == userId, cancellationToken);

        if (employer == null)
        {
            throw new NotFoundException("Employer profile not found for this user.");
        }

        var jobs = await QueryJobsAsync(employer.Id, cancellationToken);
        return jobs.Select(MapJobDto).ToList();
    }

    public async Task<IReadOnlyList<EmployerDashboardApplicationDto>> GetApplicationsForUserAsync(
        long userId,
        CancellationToken cancellationToken = default)
    {
        var employer = await _context.Employers
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.UserId == userId, cancellationToken);

        if (employer == null)
        {
            throw new NotFoundException("Employer profile not found for this user.");
        }

        var applications = await QueryApplicationsAsync(employer.Id, take: null, cancellationToken);
        return applications.Select(a => new EmployerDashboardApplicationDto
        {
            Id = a.Id,
            ApplicantName = a.ApplicantName,
            JobTitle = a.JobTitle,
            AppliedAt = a.AppliedAt,
            ResumeUrl = a.ResumeUrl,
            Status = a.Status
        }).ToList();
    }

    private async Task<List<JobRow>> QueryJobsAsync(long employerId, CancellationToken cancellationToken)
    {
        return await _context.Jobs
            .AsNoTracking()
            .Where(j => j.EmployerId == employerId)
            .OrderByDescending(j => j.Id)
            .Select(j => new JobRow
            {
                Id = j.Id,
                Title = j.Title,
                Description = j.Description,
                Location = j.Location,
                Salary = j.Salary,
                PostingStatus = j.PostingStatus,
                WorkingHours = j.WorkingHours,
                ExpiryDate = j.ExpiryDate,
                ApplicantCount = j.Applications.Count
            })
            .ToListAsync(cancellationToken);
    }

    private static EmployerDashboardJobDto MapJobDto(JobRow j) => new()
    {
        Id = j.Id,
        Title = j.Title,
        Description = j.Description,
        Location = j.Location,
        Salary = j.Salary,
        PostingStatus = j.PostingStatus,
        ApplicantCount = j.ApplicantCount,
        WorkingHours = j.WorkingHours,
        ExpiryDate = j.ExpiryDate
    };

    private async Task<List<ApplicationRow>> QueryApplicationsAsync(
        long employerId,
        int? take,
        CancellationToken cancellationToken)
    {
        var query = _context.Applications
            .AsNoTracking()
            .Where(a => a.Job.EmployerId == employerId)
            .OrderByDescending(a => a.AppliedAt)
            .Select(a => new ApplicationRow
            {
                Id = a.Id,
                AppliedAt = a.AppliedAt,
                Status = a.Status,
                ApplicantName = a.JobSeeker.Name,
                JobTitle = a.Job.Title,
                ResumeUrl = a.Resume.Url
            });

        if (take.HasValue)
        {
            return await query.Take(take.Value).ToListAsync(cancellationToken);
        }

        return await query.ToListAsync(cancellationToken);
    }

    private sealed class JobRow
    {
        public long Id { get; init; }
        public string Title { get; init; } = null!;
        public string Description { get; init; } = null!;
        public string Location { get; init; } = null!;
        public int Salary { get; init; }
        public string PostingStatus { get; init; } = null!;
        public string? WorkingHours { get; init; }
        public DateTime ExpiryDate { get; init; }
        public int ApplicantCount { get; init; }
    }

    private sealed class ApplicationRow
    {
        public long Id { get; init; }
        public DateTime AppliedAt { get; init; }
        public string Status { get; init; } = null!;
        public string ApplicantName { get; init; } = null!;
        public string JobTitle { get; init; } = null!;
        public string? ResumeUrl { get; init; }
    }
}
