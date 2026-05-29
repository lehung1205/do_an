using JobPortal.API.Data;
using JobPortal.API.DTOs;
using JobPortal.API.DTOs.Common;
using JobPortal.API.Exceptions;
using JobPortal.API.Helpers;
using JobPortal.API.Models;
using JobPortal.API.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.API.Services.Implementation;

public class EmployerDashboardService : IEmployerDashboardService
{
    private readonly AppDbContext _context;
    private readonly IJobExpiryService _jobExpiryService;
    private readonly IChatService _chatService;

    public EmployerDashboardService(
        AppDbContext context,
        IJobExpiryService jobExpiryService,
        IChatService chatService)
    {
        _context = context;
        _jobExpiryService = jobExpiryService;
        _chatService = chatService;
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
        var twoDaysAgo = now.AddDays(-2);
        var expiringThreshold = now.AddDays(3);

        var jobs = await QueryJobsAsync(employer.Id, cancellationToken);
        var employerRating = await LoadEmployerRatingForEmployerAsync(employer.Id, cancellationToken);

        var applications = await QueryApplicationsAsync(employer.Id, take: 10, cancellationToken);
        var seekerRatings = await LoadSeekerRatingsAsync(
            applications.Select(a => a.JobSeekerId),
            cancellationToken);

        var allApplicationsQuery = _context.Applications.AsNoTracking()
            .Where(a => a.Job.EmployerId == employer.Id);

        var totalCv = await allApplicationsQuery.CountAsync(cancellationToken);
        var newCv = await allApplicationsQuery.CountAsync(a => a.AppliedAt >= twoDaysAgo, cancellationToken);
        var newToday = await allApplicationsQuery.CountAsync(a => a.AppliedAt >= todayStart, cancellationToken);
        var unreadCv = await allApplicationsQuery.CountAsync(
            a => a.Status == "submitted" || a.Status == "pending",
            cancellationToken);

        var pendingJobs = jobs.Count(j =>
            string.Equals(j.PostingStatus, JobPostingCatalog.Pending, StringComparison.OrdinalIgnoreCase));
        var openJobs = jobs.Count(j =>
            string.Equals(j.PostingStatus, JobPostingCatalog.Recruiting, StringComparison.OrdinalIgnoreCase));
        var expiringSoon = jobs.Count(j =>
            string.Equals(j.PostingStatus, JobPostingCatalog.Recruiting, StringComparison.OrdinalIgnoreCase) &&
            j.ExpiryDate <= expiringThreshold &&
            j.ExpiryDate >= now);

        var notifications = new List<EmployerDashboardNotificationDto>();

        if (pendingJobs > 0)
        {
            notifications.Add(new EmployerDashboardNotificationDto
            {
                Type = "info",
                Message = $"Bạn có {pendingJobs} tin đang chờ admin duyệt"
            });
        }

        foreach (var job in jobs.Where(j =>
                     string.Equals(j.PostingStatus, JobPostingCatalog.Recruiting, StringComparison.OrdinalIgnoreCase) &&
                     j.ExpiryDate <= expiringThreshold &&
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

        var applicantChart = await BuildApplicantChartAsync(employer.Id, 7, cancellationToken);

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
                ExpiringSoonCount = expiringSoon,
                UnreadApplications = unreadCv,
                TotalJobs = jobs.Count
            },
            RecentJobs = jobs.Take(5).Select(j => MapJobDto(j, employerRating)).ToList(),
            RecentApplications = applications.Select(a => MapApplicationDto(a, seekerRatings)).ToList(),
            Notifications = notifications,
            ApplicantChart = applicantChart
        };
    }

    public async Task<EmployerPortalNavDto> GetPortalNavForUserAsync(
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

        var totalJobs = await _context.Jobs
            .AsNoTracking()
            .CountAsync(j => j.EmployerId == employer.Id, cancellationToken);

        var unreadCv = await _context.Applications
            .AsNoTracking()
            .CountAsync(
                a => a.Job.EmployerId == employer.Id &&
                     (a.Status == "submitted" || a.Status == "pending"),
                cancellationToken);

        var unreadMessages = await _chatService.GetTotalUnreadCountAsync(
            userId,
            "EMPLOYER",
            cancellationToken);

        return new EmployerPortalNavDto
        {
            CompanyName = employer.Name,
            PostingLimit = employer.PostingLimit,
            TotalJobs = totalJobs,
            UnreadApplications = unreadCv,
            UnreadMessages = unreadMessages
        };
    }

    public async Task<EmployerApplicantChartDto> GetApplicantChartForUserAsync(
        long userId,
        int days = 7,
        CancellationToken cancellationToken = default)
    {
        var employer = await _context.Employers
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.UserId == userId, cancellationToken);

        if (employer == null)
        {
            throw new NotFoundException("Employer profile not found for this user.");
        }

        days = Math.Clamp(days, 7, 30);
        return await BuildApplicantChartAsync(employer.Id, days, cancellationToken);
    }

    private async Task<EmployerApplicantChartDto> BuildApplicantChartAsync(
        long employerId,
        int days,
        CancellationToken cancellationToken)
    {
        var utcToday = DateTime.UtcNow.Date;
        var startDate = utcToday.AddDays(-(days - 1));

        var counts = await _context.Applications
            .AsNoTracking()
            .Where(a =>
                a.Job.EmployerId == employerId &&
                a.AppliedAt >= startDate)
            .GroupBy(a => a.AppliedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var countByDate = counts.ToDictionary(x => x.Date, x => x.Count);
        var points = new List<EmployerApplicantChartPointDto>();

        for (var i = 0; i < days; i++)
        {
            var date = startDate.AddDays(i);
            var count = countByDate.GetValueOrDefault(date, 0);
            points.Add(new EmployerApplicantChartPointDto
            {
                Label = date.ToString("dd/MM"),
                Count = count
            });
        }

        return new EmployerApplicantChartDto
        {
            Days = days,
            Points = points
        };
    }

    public async Task<PagedResult<EmployerDashboardJobDto>> GetJobsForUserAsync(
        long userId,
        string? status = null,
        string? search = null,
        int page = 1,
        int pageSize = 9,
        CancellationToken cancellationToken = default)
    {
        await _jobExpiryService.CloseExpiredJobsAsync(cancellationToken);

        if (page < 1)
        {
            page = 1;
        }

        if (pageSize < 1 || pageSize > 50)
        {
            pageSize = 9;
        }

        var employer = await _context.Employers
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.UserId == userId, cancellationToken);

        if (employer == null)
        {
            throw new NotFoundException("Employer profile not found for this user.");
        }

        var query = _context.Jobs
            .AsNoTracking()
            .Where(j => j.EmployerId == employer.Id);

        var statusFilter = NormalizePostingStatusFilter(status);
        if (statusFilter != null)
        {
            query = query.Where(j => j.PostingStatus == statusFilter);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(j =>
                j.Title.Contains(term) ||
                j.Description.Contains(term) ||
                j.Location.Contains(term) ||
                j.Salary.Contains(term) ||
                (j.WorkingHours != null && j.WorkingHours.Contains(term)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var jobs = await query
            .OrderByDescending(j => j.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
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
                ApplicantCount = j.Applications.Count,
                ThumbnailUrl = j.Images.OrderBy(i => i.Id).Select(i => i.Url).FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        var employerRating = await LoadEmployerRatingForEmployerAsync(employer.Id, cancellationToken);
        var items = jobs.Select(j => MapJobDto(j, employerRating)).ToList();

        return new PagedResult<EmployerDashboardJobDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<EmployerDashboardJobDto> CloseJobForUserAsync(
        long userId,
        long jobId,
        CancellationToken cancellationToken = default)
    {
        var employer = await _context.Employers
            .FirstOrDefaultAsync(e => e.UserId == userId, cancellationToken);

        if (employer == null)
        {
            throw new NotFoundException("Employer profile not found for this user.");
        }

        var job = await _context.Jobs
            .Include(j => j.Images)
            .Include(j => j.Applications)
            .FirstOrDefaultAsync(j => j.Id == jobId && j.EmployerId == employer.Id, cancellationToken);

        if (job == null)
        {
            throw new NotFoundException($"Job with id {jobId} was not found.");
        }

        var employerRating = await LoadEmployerRatingForEmployerAsync(employer.Id, cancellationToken);

        if (!string.Equals(job.PostingStatus, JobPostingCatalog.Recruiting, StringComparison.OrdinalIgnoreCase))
        {
            if (string.Equals(job.PostingStatus, JobPostingCatalog.Closed, StringComparison.OrdinalIgnoreCase))
            {
                return MapJobDto(ToJobRow(job), employerRating);
            }

            throw new BadRequestException("Chỉ có thể đóng tin đang tuyển.");
        }

        job.PostingStatus = JobPostingCatalog.Closed;
        await _context.SaveChangesAsync(cancellationToken);

        return MapJobDto(ToJobRow(job), employerRating);
    }

    private static JobRow ToJobRow(Job job) => new()
    {
        Id = job.Id,
        Title = job.Title,
        Description = job.Description,
        Location = job.Location,
        Salary = job.Salary,
        PostingStatus = job.PostingStatus,
        WorkingHours = job.WorkingHours,
        ExpiryDate = job.ExpiryDate,
        ApplicantCount = job.Applications.Count,
        ThumbnailUrl = job.Images.OrderBy(i => i.Id).Select(i => i.Url).FirstOrDefault()
    };

    private static string? NormalizePostingStatusFilter(string? status)
    {
        if (string.IsNullOrWhiteSpace(status) || string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var normalized = status.Trim().ToLowerInvariant();
        return normalized is "pending" or "recruiting" or "rejected" or "closed"
            ? normalized
            : null;
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
        var seekerRatings = await LoadSeekerRatingsAsync(
            applications.Select(a => a.JobSeekerId),
            cancellationToken);
        return applications.Select(a => MapApplicationDto(a, seekerRatings)).ToList();
    }

    public async Task<ApplicantProfileForEmployerDto> GetApplicantProfileForEmployerAsync(
        long userId,
        long applicationId,
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

        var application = await _context.Applications
            .AsNoTracking()
            .Include(a => a.JobSeeker)
            .Include(a => a.Job)
            .Include(a => a.Resume)
            .FirstOrDefaultAsync(
                a => a.Id == applicationId && a.Job.EmployerId == employer.Id,
                cancellationToken);

        if (application == null)
        {
            throw new NotFoundException($"Application with id {applicationId} was not found.");
        }

        var seeker = application.JobSeeker;

        var reviewItems = await _context.Reviews
            .AsNoTracking()
            .Where(r =>
                r.JobSeekerId == seeker.Id &&
                r.ReviewType == ReviewCatalog.EmployerToSeeker)
            .OrderByDescending(r => r.Id)
            .Select(r => new SeekerReceivedReviewItemDto
            {
                Id = r.Id,
                ApplicationId = r.ApplicationId,
                Rating = r.Rating,
                Comment = r.Comment,
                EmployerName = r.Employer.Name,
                JobTitle = r.Job.Title
            })
            .ToListAsync(cancellationToken);

        double? averageRating = reviewItems.Count == 0
            ? null
            : Math.Round(reviewItems.Average(i => i.Rating), 1);

        return new ApplicantProfileForEmployerDto
        {
            ApplicationId = application.Id,
            Name = seeker.Name,
            ProfileImage = seeker.ProfileImage,
            Email = seeker.Email,
            Phone = seeker.Phone,
            DateOfBirth = seeker.DateOfBirth,
            Gender = seeker.Gender,
            Description = seeker.Description,
            PermanentAddress = seeker.PermanentAddress,
            TemporaryAddress = seeker.TemporaryAddress,
            JobTitle = application.Job.Title,
            AppliedAt = application.AppliedAt,
            ResumeTitle = application.Resume.Title,
            ApplicationStatus = application.Status,
            Reviews = new SeekerReceivedReviewsSummaryDto
            {
                AverageRating = averageRating,
                TotalCount = reviewItems.Count,
                Items = reviewItems
            }
        };
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
            .Include(a => a.JobSeeker)
            .Include(a => a.Job).ThenInclude(j => j.Category)
            .Include(a => a.Job).ThenInclude(j => j.Images)
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

        var seekerRatings = await LoadSeekerRatingsAsync(
            new[] { application.JobSeekerId },
            cancellationToken);

        if (current is "accepted" or "rejected")
        {
            if (status == current)
            {
                return MapApplicationDto(MapApplicationRow(application), seekerRatings);
            }

            throw new BadRequestException(
                current == "accepted"
                    ? "Đơn đã được chấp nhận, không thể đổi sang trạng thái khác."
                    : "Đơn đã bị từ chối, không thể đổi sang trạng thái khác.");
        }

        application.Status = status;
        await _context.SaveChangesAsync(cancellationToken);

        if (status == "accepted")
        {
            await SeedInitialWorkProgressAsync(application.Id, cancellationToken);
        }

        return MapApplicationDto(MapApplicationRow(application), seekerRatings);
    }

    public async Task<IReadOnlyList<WorkProgressJobOptionDto>> GetWorkProgressJobOptionsAsync(
        long userId,
        CancellationToken cancellationToken = default)
    {
        await _jobExpiryService.CloseExpiredJobsAsync(cancellationToken);

        var employerId = await GetEmployerIdForUserAsync(userId, cancellationToken);

        return await _context.Applications
            .AsNoTracking()
            .Where(a => a.Job.EmployerId == employerId && a.Status == "accepted")
            .GroupBy(a => new { a.JobId, a.Job.Title })
            .Select(g => new WorkProgressJobOptionDto
            {
                JobId = g.Key.JobId,
                JobTitle = g.Key.Title,
                AcceptedCount = g.Count()
            })
            .OrderByDescending(j => j.AcceptedCount)
            .ThenBy(j => j.JobTitle)
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<EmployerAcceptedApplicationDto>> GetAcceptedApplicationsWithProgressAsync(
        long userId,
        long? jobId = null,
        string? search = null,
        string? progress = null,
        int page = 1,
        int pageSize = 9,
        CancellationToken cancellationToken = default)
    {
        await _jobExpiryService.CloseExpiredJobsAsync(cancellationToken);

        if (page < 1)
        {
            page = 1;
        }

        if (pageSize < 1 || pageSize > 50)
        {
            pageSize = 9;
        }

        if (!WorkProgressCatalog.TryNormalizeProgressFilter(progress, out var normalizedProgress))
        {
            throw new BadRequestException("Tiến độ lọc không hợp lệ.");
        }

        var employerId = await GetEmployerIdForUserAsync(userId, cancellationToken);

        var query = _context.Applications
            .AsNoTracking()
            .Where(a => a.Job.EmployerId == employerId && a.Status == "accepted");

        if (jobId is > 0)
        {
            query = query.Where(a => a.JobId == jobId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(a =>
                a.JobSeeker.Name.Contains(term) ||
                a.Job.Title.Contains(term));
        }

        query = query.FilterByLatestProgressStatus(normalizedProgress);

        var totalCount = await query.CountAsync(cancellationToken);

        var applications = await query
            .OrderByDescending(a => a.AppliedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new
            {
                a.Id,
                a.JobId,
                a.AppliedAt,
                ApplicantName = a.JobSeeker.Name,
                ApplicantProfileImage = a.JobSeeker.ProfileImage,
                JobTitle = a.Job.Title,
                Steps = a.Processes
                    .OrderByDescending(p => p.CreatedAt)
                    .ThenByDescending(p => p.Id)
                    .Select(p => new { p.Status, p.Title, p.CreatedAt })
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        var applicationIds = applications.Select(a => a.Id).ToList();
        var reviewLookup = await ApplicationReviewLookup.LoadReviewTypesByApplicationIdsAsync(
            _context.Reviews,
            applicationIds,
            cancellationToken);

        var items = applications.Select(a =>
        {
            var latest = a.Steps.FirstOrDefault();
            var isWorkFinished = WorkProgressCatalog.IsReviewableTerminalStatus(latest?.Status);
            return new EmployerAcceptedApplicationDto
            {
                ApplicationId = a.Id,
                JobId = a.JobId,
                ApplicantName = a.ApplicantName,
                ApplicantProfileImage = a.ApplicantProfileImage,
                JobTitle = a.JobTitle,
                AppliedAt = a.AppliedAt,
                CurrentWorkStatus = latest?.Status,
                CurrentWorkTitle = latest?.Title,
                LastProgressAt = latest?.CreatedAt,
                StepCount = a.Steps.Count,
                IsProgressLocked = latest != null && WorkProgressCatalog.IsLockedStatus(latest.Status),
                IsWorkFinished = isWorkFinished,
                HasSubmittedReview = isWorkFinished && ApplicationReviewLookup.HasReviewType(
                    reviewLookup,
                    a.Id,
                    ReviewCatalog.EmployerToSeeker)
            };
        }).ToList();

        return new PagedResult<EmployerAcceptedApplicationDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<ApplicationWorkProgressDto> GetApplicationWorkProgressAsync(
        long userId,
        long applicationId,
        CancellationToken cancellationToken = default)
    {
        await _jobExpiryService.CloseExpiredJobsAsync(cancellationToken);

        var application = await GetAcceptedApplicationForEmployerAsync(userId, applicationId, cancellationToken);

        var steps = await _context.Processes
            .AsNoTracking()
            .Where(p => p.ApplicationId == applicationId)
            .OrderByDescending(p => p.CreatedAt)
            .ThenByDescending(p => p.Id)
            .ToListAsync(cancellationToken);

        var stepDtos = steps.Select(MapWorkProgressStep).ToList();
        var currentStep = stepDtos.FirstOrDefault();

        return new ApplicationWorkProgressDto
        {
            ApplicationId = application.Id,
            JobId = application.JobId,
            ApplicantName = application.JobSeeker.Name,
            ApplicantProfileImage = application.JobSeeker.ProfileImage,
            ApplicantEmail = application.JobSeeker.Email,
            ApplicantPhone = application.JobSeeker.Phone,
            JobTitle = application.Job.Title,
            AppliedAt = application.AppliedAt,
            ApplicationStatus = application.Status,
            Steps = stepDtos,
            CurrentStep = currentStep,
            IsProgressLocked = WorkProgressCatalog.IsLockedStatus(currentStep?.Status)
        };
    }

    public async Task<WorkProgressStepDto> AddWorkProgressStepAsync(
        long userId,
        long applicationId,
        CreateWorkProgressStepRequest request,
        CancellationToken cancellationToken = default)
    {
        await _jobExpiryService.CloseExpiredJobsAsync(cancellationToken);

        await GetAcceptedApplicationForEmployerAsync(userId, applicationId, cancellationToken);

        var latestStep = await _context.Processes
            .AsNoTracking()
            .Where(p => p.ApplicationId == applicationId)
            .OrderByDescending(p => p.CreatedAt)
            .ThenByDescending(p => p.Id)
            .Select(p => p.Status)
            .FirstOrDefaultAsync(cancellationToken);

        if (WorkProgressCatalog.IsLockedStatus(latestStep))
        {
            throw new BadRequestException("Tiến độ đã kết thúc (hoàn thành hoặc đã hủy), không thể cập nhật thêm.");
        }

        var status = request.Status.Trim().ToLowerInvariant();
        if (!WorkProgressCatalog.IsValidStatus(status))
        {
            throw new BadRequestException("Trạng thái tiến độ không hợp lệ.");
        }

        var now = DateTime.UtcNow;
        var entity = new Process
        {
            ApplicationId = applicationId,
            Status = status,
            Title = WorkProgressCatalog.GetTitle(status),
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            CreatedAt = now,
            UpdatedAt = null
        };

        _context.Processes.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return MapWorkProgressStep(entity);
    }

    private async Task SeedInitialWorkProgressAsync(long applicationId, CancellationToken cancellationToken)
    {
        var hasSteps = await _context.Processes
            .AnyAsync(p => p.ApplicationId == applicationId, cancellationToken);

        if (hasSteps)
        {
            return;
        }

        _context.Processes.Add(new Process
        {
            ApplicationId = applicationId,
            Status = "confirmed",
            Title = WorkProgressCatalog.GetTitle("confirmed"),
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<long> GetEmployerIdForUserAsync(long userId, CancellationToken cancellationToken)
    {
        var employerId = await _context.Employers
            .AsNoTracking()
            .Where(e => e.UserId == userId)
            .Select(e => e.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (employerId == 0)
        {
            throw new NotFoundException("Employer profile not found for this user.");
        }

        return employerId;
    }

    private async Task<Application> GetAcceptedApplicationForEmployerAsync(
        long userId,
        long applicationId,
        CancellationToken cancellationToken)
    {
        var employerId = await GetEmployerIdForUserAsync(userId, cancellationToken);

        var application = await _context.Applications
            .AsNoTracking()
            .Include(a => a.Job)
            .Include(a => a.JobSeeker)
            .FirstOrDefaultAsync(
                a => a.Id == applicationId && a.Job.EmployerId == employerId,
                cancellationToken);

        if (application == null)
        {
            throw new NotFoundException($"Application with id {applicationId} was not found.");
        }

        if (!string.Equals(application.Status, "accepted", StringComparison.OrdinalIgnoreCase))
        {
            throw new BadRequestException("Chỉ quản lý tiến độ cho ứng viên đã được chấp nhận.");
        }

        return application;
    }

    private static WorkProgressStepDto MapWorkProgressStep(Process p) => new()
    {
        Id = p.Id,
        ApplicationId = p.ApplicationId,
        Status = p.Status,
        Title = p.Title,
        Notes = p.Notes,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt
    };

    private static ApplicationRow MapApplicationRow(Application application)
    {
        var job = application.Job;
        return new ApplicationRow
        {
            Id = application.Id,
            JobId = application.JobId,
            JobSeekerId = application.JobSeekerId,
            AppliedAt = application.AppliedAt,
            Status = application.Status,
            ApplicantName = application.JobSeeker.Name,
            ApplicantEmail = application.JobSeeker.Email,
            ApplicantPhone = application.JobSeeker.Phone,
            ApplicantProfileImage = application.JobSeeker.ProfileImage,
            JobTitle = job.Title,
            CategoryName = job.Category?.Name,
            JobLocation = job.Location,
            JobSalary = job.Salary,
            JobWorkingHours = job.WorkingHours,
            JobExpiryDate = job.ExpiryDate,
            JobPostingStatus = job.PostingStatus,
            JobThumbnailUrl = job.Images?.OrderBy(i => i.Id).Select(i => i.Url).FirstOrDefault(),
            ResumeId = application.ResumeId,
            ResumeTitle = application.Resume.Title,
            ResumeUrl = application.Resume.Url
        };
    }

    private static EmployerDashboardApplicationDto MapApplicationDto(
        ApplicationRow a,
        IReadOnlyDictionary<long, SeekerRatingSnapshot>? seekerRatings = null)
    {
        SeekerRatingSnapshot? rating = null;
        if (seekerRatings != null && seekerRatings.TryGetValue(a.JobSeekerId, out var snapshot))
        {
            rating = snapshot;
        }

        return new EmployerDashboardApplicationDto
        {
            Id = a.Id,
            JobId = a.JobId,
            ApplicantName = a.ApplicantName,
            ApplicantEmail = a.ApplicantEmail,
            ApplicantPhone = a.ApplicantPhone,
            ApplicantProfileImage = a.ApplicantProfileImage,
            ApplicantAverageRating = rating?.Average,
            ApplicantReviewCount = rating?.Count ?? 0,
            JobTitle = a.JobTitle,
            CategoryName = a.CategoryName,
            JobLocation = a.JobLocation,
            JobSalary = a.JobSalary,
            JobWorkingHours = a.JobWorkingHours,
            JobExpiryDate = a.JobExpiryDate,
            JobPostingStatus = a.JobPostingStatus,
            JobThumbnailUrl = a.JobThumbnailUrl,
            AppliedAt = a.AppliedAt,
            ResumeId = a.ResumeId,
            ResumeTitle = a.ResumeTitle,
            ResumeUrl = a.ResumeUrl,
            Status = a.Status,
            IsUnread = IsUnreadStatus(a.Status)
        };
    }

    private async Task<Dictionary<long, SeekerRatingSnapshot>> LoadSeekerRatingsAsync(
        IEnumerable<long> jobSeekerIds,
        CancellationToken cancellationToken)
    {
        var ids = jobSeekerIds.Distinct().ToList();
        if (ids.Count == 0)
        {
            return new Dictionary<long, SeekerRatingSnapshot>();
        }

        var rows = await _context.Reviews
            .AsNoTracking()
            .Where(r =>
                r.ReviewType == ReviewCatalog.EmployerToSeeker &&
                ids.Contains(r.JobSeekerId))
            .GroupBy(r => r.JobSeekerId)
            .Select(g => new
            {
                JobSeekerId = g.Key,
                Average = g.Average(x => (double)x.Rating),
                Count = g.Count()
            })
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(
            x => x.JobSeekerId,
            x => new SeekerRatingSnapshot
            {
                Average = Math.Round(x.Average, 1),
                Count = x.Count
            });
    }

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
                ApplicantCount = j.Applications.Count,
                ThumbnailUrl = j.Images.OrderBy(i => i.Id).Select(i => i.Url).FirstOrDefault()
            })
            .ToListAsync(cancellationToken);
    }

    private static EmployerDashboardJobDto MapJobDto(
        JobRow j,
        EmployerRatingSnapshot? employerRating = null) => new()
    {
        Id = j.Id,
        Title = j.Title,
        Description = j.Description,
        Location = j.Location,
        Salary = j.Salary,
        PostingStatus = j.PostingStatus,
        EmployerAverageRating = employerRating?.Average,
        EmployerReviewCount = employerRating?.Count ?? 0,
        ApplicantCount = j.ApplicantCount,
        WorkingHours = j.WorkingHours,
        ExpiryDate = j.ExpiryDate,
        ThumbnailUrl = j.ThumbnailUrl
    };

    private async Task<EmployerRatingSnapshot?> LoadEmployerRatingForEmployerAsync(
        long employerId,
        CancellationToken cancellationToken)
    {
        var ratings = await LoadEmployerRatingsAsync(new[] { employerId }, cancellationToken);
        return ratings.TryGetValue(employerId, out var rating) ? rating : null;
    }

    private async Task<Dictionary<long, EmployerRatingSnapshot>> LoadEmployerRatingsAsync(
        IEnumerable<long> employerIds,
        CancellationToken cancellationToken)
    {
        var ids = employerIds.Distinct().ToList();
        if (ids.Count == 0)
        {
            return new Dictionary<long, EmployerRatingSnapshot>();
        }

        var rows = await _context.Reviews
            .AsNoTracking()
            .Where(r =>
                r.ReviewType == ReviewCatalog.SeekerToEmployer &&
                ids.Contains(r.EmployerId))
            .GroupBy(r => r.EmployerId)
            .Select(g => new
            {
                EmployerId = g.Key,
                Average = g.Average(x => (double)x.Rating),
                Count = g.Count()
            })
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(
            x => x.EmployerId,
            x => new EmployerRatingSnapshot
            {
                Average = Math.Round(x.Average, 1),
                Count = x.Count
            });
    }

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
                JobSeekerId = a.JobSeekerId,
                AppliedAt = a.AppliedAt,
                Status = a.Status,
                ApplicantName = a.JobSeeker.Name,
                ApplicantEmail = a.JobSeeker.Email,
                ApplicantPhone = a.JobSeeker.Phone,
                ApplicantProfileImage = a.JobSeeker.ProfileImage,
                JobTitle = a.Job.Title,
                CategoryName = a.Job.Category.Name,
                JobLocation = a.Job.Location,
                JobSalary = a.Job.Salary,
                JobWorkingHours = a.Job.WorkingHours,
                JobExpiryDate = a.Job.ExpiryDate,
                JobPostingStatus = a.Job.PostingStatus,
                JobThumbnailUrl = a.Job.Images.OrderBy(i => i.Id).Select(i => i.Url).FirstOrDefault(),
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
        public string Salary { get; init; } = null!;
        public string PostingStatus { get; init; } = null!;
        public string? WorkingHours { get; init; }
        public DateTime ExpiryDate { get; init; }
        public int ApplicantCount { get; init; }
        public string? ThumbnailUrl { get; init; }
    }

    private sealed class SeekerRatingSnapshot
    {
        public double Average { get; init; }
        public int Count { get; init; }
    }

    private sealed class EmployerRatingSnapshot
    {
        public double Average { get; init; }
        public int Count { get; init; }
    }

    private sealed class ApplicationRow
    {
        public long Id { get; init; }
        public long JobId { get; init; }
        public long JobSeekerId { get; init; }
        public DateTime AppliedAt { get; init; }
        public string Status { get; init; } = null!;
        public string ApplicantName { get; init; } = null!;
        public string? ApplicantEmail { get; init; }
        public string? ApplicantPhone { get; init; }
        public string? ApplicantProfileImage { get; init; }
        public string JobTitle { get; init; } = null!;
        public string? CategoryName { get; init; }
        public string JobLocation { get; init; } = null!;
        public string JobSalary { get; init; } = null!;
        public string? JobWorkingHours { get; init; }
        public DateTime JobExpiryDate { get; init; }
        public string JobPostingStatus { get; init; } = null!;
        public string? JobThumbnailUrl { get; init; }
        public long ResumeId { get; init; }
        public string ResumeTitle { get; init; } = null!;
        public string? ResumeUrl { get; init; }
    }
}
