using JobPortal.API.Data;
using JobPortal.API.DTOs;
using JobPortal.API.Exceptions;
using JobPortal.API.Helpers;
using JobPortal.API.Models;
using JobPortal.API.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.API.Services.Implementation;

public class ApplicationReviewService : IApplicationReviewService
{
    private readonly AppDbContext _context;
    private readonly INotificationService _notificationService;

    public ApplicationReviewService(AppDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<ApplicationReviewContextDto> GetEmployerReviewContextAsync(
        long userId,
        long applicationId,
        CancellationToken cancellationToken = default)
    {
        var application = await LoadAcceptedApplicationForEmployerAsync(userId, applicationId, cancellationToken);
        var latestStatus = await GetLatestWorkStatusAsync(applicationId, cancellationToken);
        var isWorkFinished = WorkProgressCatalog.IsReviewableTerminalStatus(latestStatus);
        var reviews = await LoadApplicationReviewsAsync(applicationId, cancellationToken);

        var myReview = reviews.FirstOrDefault(r => r.ReviewType == ReviewCatalog.EmployerToSeeker);
        var receivedReview = reviews.FirstOrDefault(r => r.ReviewType == ReviewCatalog.SeekerToEmployer);

        return new ApplicationReviewContextDto
        {
            ApplicationId = applicationId,
            IsWorkFinished = isWorkFinished,
            CanSubmitReview = isWorkFinished && myReview == null,
            TargetLabel = application.JobSeeker.Name,
            MyReview = myReview == null ? null : MapReviewView(myReview),
            ReceivedReview = receivedReview == null ? null : MapReviewView(receivedReview)
        };
    }

    public async Task<ApplicationReviewViewDto> SubmitEmployerReviewAsync(
        long userId,
        long applicationId,
        CreateApplicationReviewRequest request,
        CancellationToken cancellationToken = default)
    {
        var application = await LoadAcceptedApplicationForEmployerAsync(userId, applicationId, cancellationToken);
        await EnsureCanSubmitReviewAsync(
            applicationId,
            ReviewCatalog.EmployerToSeeker,
            "Bạn đã đánh giá ứng viên cho đơn này.",
            request,
            cancellationToken);

        var employer = await _context.Employers
            .AsNoTracking()
            .FirstAsync(e => e.UserId == userId, cancellationToken);

        var entity = new Review
        {
            ApplicationId = applicationId,
            JobId = application.JobId,
            EmployerId = employer.Id,
            JobSeekerId = application.JobSeekerId,
            Rating = request.Rating,
            Comment = NormalizeComment(request.Comment),
            ReviewType = ReviewCatalog.EmployerToSeeker
        };

        _context.Reviews.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        await _notificationService.NotifyReviewReceivedAsync(
            application.JobSeeker.UserId,
            applicationId,
            employer.Name,
            application.Job.Title,
            request.Rating,
            cancellationToken);

        return new ApplicationReviewViewDto
        {
            Id = entity.Id,
            Rating = entity.Rating,
            Comment = entity.Comment,
            ReviewType = entity.ReviewType,
            ReviewerName = employer.Name
        };
    }

    public async Task<ApplicationReviewContextDto> GetSeekerReviewContextAsync(
        long userId,
        long applicationId,
        CancellationToken cancellationToken = default)
    {
        var application = await LoadAcceptedApplicationForSeekerAsync(userId, applicationId, cancellationToken);
        var latestStatus = await GetLatestWorkStatusAsync(applicationId, cancellationToken);
        var isWorkFinished = WorkProgressCatalog.IsReviewableTerminalStatus(latestStatus);
        var reviews = await LoadApplicationReviewsAsync(applicationId, cancellationToken);

        var myReview = reviews.FirstOrDefault(r => r.ReviewType == ReviewCatalog.SeekerToEmployer);
        var receivedReview = reviews.FirstOrDefault(r => r.ReviewType == ReviewCatalog.EmployerToSeeker);

        return new ApplicationReviewContextDto
        {
            ApplicationId = applicationId,
            IsWorkFinished = isWorkFinished,
            CanSubmitReview = isWorkFinished && myReview == null,
            TargetLabel = application.Job.Employer.Name,
            MyReview = myReview == null ? null : MapReviewView(myReview),
            ReceivedReview = receivedReview == null ? null : MapReviewView(receivedReview)
        };
    }

    public async Task<ApplicationReviewViewDto> SubmitSeekerReviewAsync(
        long userId,
        long applicationId,
        CreateApplicationReviewRequest request,
        CancellationToken cancellationToken = default)
    {
        var application = await LoadAcceptedApplicationForSeekerAsync(userId, applicationId, cancellationToken);
        await EnsureCanSubmitReviewAsync(
            applicationId,
            ReviewCatalog.SeekerToEmployer,
            "Bạn đã đánh giá nhà tuyển dụng cho đơn này.",
            request,
            cancellationToken);

        var jobSeeker = await _context.JobSeekers
            .AsNoTracking()
            .FirstAsync(js => js.UserId == userId, cancellationToken);

        var entity = new Review
        {
            ApplicationId = applicationId,
            JobId = application.JobId,
            EmployerId = application.Job.EmployerId,
            JobSeekerId = jobSeeker.Id,
            Rating = request.Rating,
            Comment = NormalizeComment(request.Comment),
            ReviewType = ReviewCatalog.SeekerToEmployer
        };

        _context.Reviews.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        await _notificationService.NotifyReviewReceivedAsync(
            application.Job.Employer.UserId,
            applicationId,
            jobSeeker.Name,
            application.Job.Title,
            request.Rating,
            cancellationToken);

        return new ApplicationReviewViewDto
        {
            Id = entity.Id,
            Rating = entity.Rating,
            Comment = entity.Comment,
            ReviewType = entity.ReviewType,
            ReviewerName = jobSeeker.Name
        };
    }

    private async Task<Application> LoadAcceptedApplicationForEmployerAsync(
        long userId,
        long applicationId,
        CancellationToken cancellationToken)
    {
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
            .FirstOrDefaultAsync(
                a => a.Id == applicationId && a.Job.EmployerId == employer.Id,
                cancellationToken);

        if (application == null)
        {
            throw new NotFoundException($"Application with id {applicationId} was not found.");
        }

        if (!string.Equals(application.Status, "accepted", StringComparison.OrdinalIgnoreCase))
        {
            throw new BadRequestException("Chỉ đánh giá cho ứng viên đã được chấp nhận.");
        }

        return application;
    }

    private async Task<Application> LoadAcceptedApplicationForSeekerAsync(
        long userId,
        long applicationId,
        CancellationToken cancellationToken)
    {
        var jobSeeker = await _context.JobSeekers
            .AsNoTracking()
            .FirstOrDefaultAsync(js => js.UserId == userId, cancellationToken);

        if (jobSeeker == null)
        {
            throw new NotFoundException("Job seeker profile not found for this user.");
        }

        var application = await _context.Applications
            .AsNoTracking()
            .Include(a => a.JobSeeker)
            .Include(a => a.Job)
            .ThenInclude(j => j.Employer)
            .FirstOrDefaultAsync(
                a => a.Id == applicationId && a.JobSeekerId == jobSeeker.Id,
                cancellationToken);

        if (application == null)
        {
            throw new NotFoundException($"Application with id {applicationId} was not found.");
        }

        if (!string.Equals(application.Status, "accepted", StringComparison.OrdinalIgnoreCase))
        {
            throw new BadRequestException("Chỉ đánh giá cho đơn ứng tuyển đã được chấp nhận.");
        }

        return application;
    }

    private async Task EnsureCanSubmitReviewAsync(
        long applicationId,
        string reviewType,
        string duplicateMessage,
        CreateApplicationReviewRequest request,
        CancellationToken cancellationToken)
    {
        var latestStatus = await GetLatestWorkStatusAsync(applicationId, cancellationToken);

        if (!WorkProgressCatalog.IsReviewableTerminalStatus(latestStatus))
        {
            throw new BadRequestException("Chỉ có thể đánh giá sau khi tiến độ là Hoàn thành hoặc Đã hủy.");
        }

        var alreadyExists = await _context.Reviews
            .AnyAsync(r => r.ApplicationId == applicationId && r.ReviewType == reviewType, cancellationToken);

        if (alreadyExists)
        {
            throw new BadRequestException(duplicateMessage);
        }

        if (request.Rating < ReviewCatalog.MinRating || request.Rating > ReviewCatalog.MaxRating)
        {
            throw new BadRequestException($"Điểm đánh giá phải từ {ReviewCatalog.MinRating} đến {ReviewCatalog.MaxRating}.");
        }

        var comment = NormalizeComment(request.Comment);
        if (comment != null && comment.Length > ReviewCatalog.MaxCommentLength)
        {
            throw new BadRequestException($"Nhận xét tối đa {ReviewCatalog.MaxCommentLength} ký tự.");
        }
    }

    private static string? NormalizeComment(string? comment) =>
        string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();

    private async Task<List<ReviewRow>> LoadApplicationReviewsAsync(
        long applicationId,
        CancellationToken cancellationToken) =>
        await _context.Reviews
            .AsNoTracking()
            .Where(r => r.ApplicationId == applicationId)
            .Select(r => new ReviewRow
            {
                Id = r.Id,
                Rating = r.Rating,
                Comment = r.Comment,
                ReviewType = r.ReviewType,
                ReviewerName = r.ReviewType == ReviewCatalog.EmployerToSeeker
                    ? r.Employer.Name
                    : r.JobSeeker.Name
            })
            .ToListAsync(cancellationToken);

    private async Task<string?> GetLatestWorkStatusAsync(long applicationId, CancellationToken cancellationToken) =>
        await _context.Processes
            .AsNoTracking()
            .Where(p => p.ApplicationId == applicationId)
            .OrderByDescending(p => p.CreatedAt)
            .ThenByDescending(p => p.Id)
            .Select(p => p.Status)
            .FirstOrDefaultAsync(cancellationToken);

    private static ApplicationReviewViewDto MapReviewView(ReviewRow row) => new()
    {
        Id = row.Id,
        Rating = row.Rating,
        Comment = row.Comment,
        ReviewType = row.ReviewType,
        ReviewerName = row.ReviewerName
    };

    public async Task<SeekerReceivedReviewsSummaryDto> GetSeekerReceivedReviewsAsync(
        long userId,
        CancellationToken cancellationToken = default)
    {
        var jobSeeker = await _context.JobSeekers
            .AsNoTracking()
            .FirstOrDefaultAsync(js => js.UserId == userId, cancellationToken);

        if (jobSeeker == null)
        {
            throw new NotFoundException("Job seeker profile not found for this user.");
        }

        var items = await _context.Reviews
            .AsNoTracking()
            .Where(r =>
                r.JobSeekerId == jobSeeker.Id &&
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

        double? average = items.Count == 0
            ? null
            : Math.Round(items.Average(i => i.Rating), 1);

        return new SeekerReceivedReviewsSummaryDto
        {
            AverageRating = average,
            TotalCount = items.Count,
            Items = items
        };
    }

    public async Task<EmployerReceivedReviewsSummaryDto> GetEmployerReceivedReviewsAsync(
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

        var items = await _context.Reviews
            .AsNoTracking()
            .Where(r =>
                r.EmployerId == employer.Id &&
                r.ReviewType == ReviewCatalog.SeekerToEmployer)
            .OrderByDescending(r => r.Id)
            .Select(r => new EmployerReceivedReviewItemDto
            {
                Id = r.Id,
                ApplicationId = r.ApplicationId,
                Rating = r.Rating,
                Comment = r.Comment,
                ApplicantName = r.JobSeeker.Name,
                JobTitle = r.Job.Title
            })
            .ToListAsync(cancellationToken);

        double? average = items.Count == 0
            ? null
            : Math.Round(items.Average(i => i.Rating), 1);

        return new EmployerReceivedReviewsSummaryDto
        {
            AverageRating = average,
            TotalCount = items.Count,
            Items = items
        };
    }

    private sealed class ReviewRow
    {
        public long Id { get; init; }
        public int Rating { get; init; }
        public string? Comment { get; init; }
        public string ReviewType { get; init; } = null!;
        public string ReviewerName { get; init; } = null!;
    }
}
