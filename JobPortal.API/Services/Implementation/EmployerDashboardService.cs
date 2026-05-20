using JobPortal.API.Data;
using JobPortal.API.DTOs;
using JobPortal.API.Exceptions;
using JobPortal.API.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.API.Services.Implementation;

public class EmployerDashboardService : IEmployerDashboardService
{
    private readonly AppDbContext _context;
    private readonly IJobExpiryService _jobExpiryService;

    public EmployerDashboardService(AppDbContext context, IJobExpiryService jobExpiryService)
    {
        _context = context;
        _jobExpiryService = jobExpiryService;
    }

    public async Task<EmployerDashboardDto> GetDashboardForUserAsync(long userId, CancellationToken cancellationToken = default)
    {
        await _jobExpiryService.CloseExpiredJobsAsync(cancellationToken);

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
            a => a.Status == "submitted" || a.Status == "pending",
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
            RecentApplications = applications.Select(MapApplicationDto).ToList(),
            Notifications = notifications
        };
    }

    public async Task<IReadOnlyList<EmployerDashboardJobDto>> GetJobsForUserAsync(
        long userId,
        CancellationToken cancellationToken = default)
    {
        await _jobExpiryService.CloseExpiredJobsAsync(cancellationToken);

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
        await _jobExpiryService.CloseExpiredJobsAsync(cancellationToken);

        var employer = await _context.Employers
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.UserId == userId, cancellationToken);

        if (employer == null)
        {
            throw new NotFoundException("Employer profile not found for this user.");
        }

        var applications = await QueryApplicationsAsync(employer.Id, take: null, cancellationToken);
        return applications.Select(MapApplicationDto).ToList();
    }

    public async Task<EmployerDashboardApplicationDto> UpdateApplicationStatusAsync(
        long userId,
        long applicationId,
        UpdateEmployerApplicationStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        await _jobExpiryService.CloseExpiredJobsAsync(cancellationToken);

        var employer = await _context.Employers
            .FirstOrDefaultAsync(e => e.UserId == userId, cancellationToken);

        if (employer == null)
        {
            throw new NotFoundException("Employer profile not found for this user.");
        }

        var application = await _context.Applications
            .Include(a => a.Job)
            .Include(a => a.JobSeeker)
            .Include(a => a.Resume)
            .FirstOrDefaultAsync(
                a => a.Id == applicationId && a.Job.EmployerId == employer.Id,
                cancellationToken);

        if (application == null)
        {
            throw new NotFoundException($"Application with id {applicationId} was not found.");
        }

        var current = application.Status.Trim().ToLowerInvariant();
        var status = request.Status.Trim().ToLowerInvariant();

        if (current is "accepted" or "rejected")
        {
            if (status == current)
            {
                return MapApplicationDto(new ApplicationRow
                {
                    Id = application.Id,
                    JobId = application.JobId,
                    AppliedAt = application.AppliedAt,
                    Status = application.Status,
                    ApplicantName = application.JobSeeker.Name,
                    ApplicantEmail = application.JobSeeker.Email,
                    ApplicantPhone = application.JobSeeker.Phone,
                    JobTitle = application.Job.Title,
                    ResumeId = application.ResumeId,
                    ResumeTitle = application.Resume.Title,
                    ResumeUrl = application.Resume.Url
                });
            }

            throw new BadRequestException(
                current == "accepted"
                    ? "Đơn đã được chấp nhận, không thể đổi sang trạng thái khác."
                    : "Đơn đã bị từ chối, không thể đổi sang trạng thái khác.");
        }

        application.Status = status;
        await _context.SaveChangesAsync(cancellationToken);

        return MapApplicationDto(new ApplicationRow
        {
            Id = application.Id,
            JobId = application.JobId,
            AppliedAt = application.AppliedAt,
            Status = application.Status,
            ApplicantName = application.JobSeeker.Name,
            ApplicantEmail = application.JobSeeker.Email,
            ApplicantPhone = application.JobSeeker.Phone,
            JobTitle = application.Job.Title,
            ResumeId = application.ResumeId,
            ResumeTitle = application.Resume.Title,
            ResumeUrl = application.Resume.Url
        });
    }

    private static EmployerDashboardApplicationDto MapApplicationDto(ApplicationRow a) => new()
    {
        Id = a.Id,
        JobId = a.JobId,
        ApplicantName = a.ApplicantName,
        ApplicantEmail = a.ApplicantEmail,
        ApplicantPhone = a.ApplicantPhone,
        JobTitle = a.JobTitle,
        AppliedAt = a.AppliedAt,
        ResumeId = a.ResumeId,
        ResumeTitle = a.ResumeTitle,
        ResumeUrl = a.ResumeUrl,
        Status = a.Status,
        IsUnread = IsUnreadStatus(a.Status)
    };

    private static bool IsUnreadStatus(string status)
    {
        var s = status.Trim().ToLowerInvariant();
        return s is "submitted" or "pending";
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
                JobId = a.JobId,
                AppliedAt = a.AppliedAt,
                Status = a.Status,
                ApplicantName = a.JobSeeker.Name,
                ApplicantEmail = a.JobSeeker.Email,
                ApplicantPhone = a.JobSeeker.Phone,
                JobTitle = a.Job.Title,
                ResumeId = a.ResumeId,
                ResumeTitle = a.Resume.Title,
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
        public long JobId { get; init; }
        public DateTime AppliedAt { get; init; }
        public string Status { get; init; } = null!;
        public string ApplicantName { get; init; } = null!;
        public string? ApplicantEmail { get; init; }
        public string? ApplicantPhone { get; init; }
        public string JobTitle { get; init; } = null!;
        public long ResumeId { get; init; }
        public string ResumeTitle { get; init; } = null!;
        public string? ResumeUrl { get; init; }
    }
}
