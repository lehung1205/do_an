using JobPortal.API.Data;
using JobPortal.API.DTOs;
using JobPortal.API.DTOs.Common;
using JobPortal.API.Exceptions;
using JobPortal.API.Models;
using JobPortal.API.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.API.Services.Implementation;

public class ChatService : IChatService
{
    private const int MaxContentLength = 2000;
    private const int DefaultPageSize = 50;
    private const int MaxPageSize = 100;

    private readonly AppDbContext _context;
    private readonly IChatPresenceService _presence;

    public ChatService(AppDbContext context, IChatPresenceService presence)
    {
        _context = context;
        _presence = presence;
    }

    public async Task<IReadOnlyList<ChatThreadDto>> GetThreadsForUserAsync(
        long userId,
        string role,
        CancellationToken cancellationToken = default)
    {
        var normalizedRole = NormalizeRole(role);

        if (normalizedRole == "EMPLOYER")
        {
            var employerId = await GetEmployerIdForUserAsync(userId, cancellationToken);
            var applications = await _context.Applications
                .AsNoTracking()
                .Where(a => a.Job.EmployerId == employerId)
                .OrderByDescending(a => a.AppliedAt)
                .Select(a => new
                {
                    a.Id,
                    a.AppliedAt,
                    Title = a.JobSeeker.Name,
                    Subtitle = a.Job.Title,
                    PartnerUserId = a.JobSeeker.UserId
                })
                .ToListAsync(cancellationToken);

            return OrderThreads(await BuildThreadsAsync(
                applications.Select(a => (a.Id, a.AppliedAt, a.Title, a.Subtitle, a.PartnerUserId)).ToList(),
                userId,
                cancellationToken));
        }

        if (normalizedRole == "JOB_SEEKER")
        {
            var seekerId = await GetJobSeekerIdForUserAsync(userId, cancellationToken);
            var applications = await _context.Applications
                .AsNoTracking()
                .Where(a => a.JobSeekerId == seekerId)
                .OrderByDescending(a => a.AppliedAt)
                .Select(a => new
                {
                    a.Id,
                    a.AppliedAt,
                    Title = a.Job.Title,
                    Subtitle = a.Job.Employer.Name,
                    PartnerUserId = a.Job.Employer.UserId
                })
                .ToListAsync(cancellationToken);

            return OrderThreads(await BuildThreadsAsync(
                applications.Select(a => (a.Id, a.AppliedAt, a.Title, a.Subtitle, a.PartnerUserId)).ToList(),
                userId,
                cancellationToken));
        }

        throw new ForbiddenException("Chỉ nhà tuyển dụng hoặc ứng viên mới được dùng tin nhắn.");
    }

    public async Task<int> GetTotalUnreadCountAsync(long userId, string role, CancellationToken cancellationToken = default)
    {
        var normalizedRole = NormalizeRole(role);

        if (normalizedRole == "EMPLOYER")
        {
            var employerId = await GetEmployerIdForUserAsync(userId, cancellationToken);
            return await _context.ChatMessages
                .AsNoTracking()
                .Where(m => m.ReadAt == null && m.SenderUserId != userId)
                .Where(m => m.Application.Job.EmployerId == employerId)
                .CountAsync(cancellationToken);
        }

        if (normalizedRole == "JOB_SEEKER")
        {
            var seekerId = await GetJobSeekerIdForUserAsync(userId, cancellationToken);
            return await _context.ChatMessages
                .AsNoTracking()
                .Where(m => m.ReadAt == null && m.SenderUserId != userId)
                .Where(m => m.Application.JobSeekerId == seekerId)
                .CountAsync(cancellationToken);
        }

        return 0;
    }

    public async Task<PagedResult<ChatMessageDto>> GetMessagesAsync(
        long userId,
        string role,
        long applicationId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        await EnsureCanAccessApplicationAsync(userId, role, applicationId, cancellationToken);

        if (page < 1)
        {
            page = 1;
        }

        if (pageSize < 1)
        {
            pageSize = DefaultPageSize;
        }

        if (pageSize > MaxPageSize)
        {
            pageSize = MaxPageSize;
        }

        var query = _context.ChatMessages
            .AsNoTracking()
            .Include(m => m.Sender)
            .Where(m => m.ApplicationId == applicationId)
            .OrderByDescending(m => m.SentAt);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        items.Reverse();

        var totalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);

        return new PagedResult<ChatMessageDto>
        {
            Items = items.Select(m => MapMessage(m, userId)).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = total,
            TotalPages = totalPages
        };
    }

    public async Task<ChatMessageDto> SendMessageAsync(
        long userId,
        string role,
        long applicationId,
        string content,
        CancellationToken cancellationToken = default)
    {
        await EnsureCanAccessApplicationAsync(userId, role, applicationId, cancellationToken);

        var trimmed = content?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(trimmed))
        {
            throw new BadRequestException("Nội dung tin nhắn không được để trống.");
        }

        if (trimmed.Length > MaxContentLength)
        {
            throw new BadRequestException($"Tin nhắn tối đa {MaxContentLength} ký tự.");
        }

        var entity = new ChatMessage
        {
            ApplicationId = applicationId,
            SenderUserId = userId,
            Content = trimmed,
            SentAt = DateTime.UtcNow
        };

        _context.ChatMessages.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        await _context.Entry(entity).Reference(m => m.Sender).LoadAsync(cancellationToken);

        return MapMessage(entity, userId);
    }

    public async Task MarkAsReadAsync(
        long userId,
        string role,
        long applicationId,
        CancellationToken cancellationToken = default)
    {
        await EnsureCanAccessApplicationAsync(userId, role, applicationId, cancellationToken);

        var unread = await _context.ChatMessages
            .Where(m => m.ApplicationId == applicationId
                && m.SenderUserId != userId
                && m.ReadAt == null)
            .ToListAsync(cancellationToken);

        if (unread.Count == 0)
        {
            return;
        }

        var now = DateTime.UtcNow;
        foreach (var message in unread)
        {
            message.ReadAt = now;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<ChatJoinedDto> GetThreadInfoAsync(
        long userId,
        string role,
        long applicationId,
        CancellationToken cancellationToken = default)
    {
        var access = await EnsureCanAccessApplicationAsync(userId, role, applicationId, cancellationToken);
        return new ChatJoinedDto
        {
            ApplicationId = applicationId,
            PartnerUserId = access.PartnerUserId,
            PartnerName = access.PartnerName,
            JobTitle = access.JobTitle,
            PartnerIsOnline = _presence.IsUserOnline(access.PartnerUserId)
        };
    }

    private async Task<IReadOnlyList<ChatThreadDto>> BuildThreadsAsync(
        IReadOnlyList<(long Id, DateTime AppliedAt, string Title, string Subtitle, long PartnerUserId)> applications,
        long userId,
        CancellationToken cancellationToken)
    {
        if (applications.Count == 0)
        {
            return Array.Empty<ChatThreadDto>();
        }

        var ids = applications.Select(a => a.Id).ToList();

        var recentMessages = await _context.ChatMessages
            .AsNoTracking()
            .Where(m => ids.Contains(m.ApplicationId))
            .OrderByDescending(m => m.SentAt)
            .ToListAsync(cancellationToken);

        var lastMessages = recentMessages
            .GroupBy(m => m.ApplicationId)
            .ToDictionary(g => g.Key, g => g.First());

        var unreadCounts = await _context.ChatMessages
            .AsNoTracking()
            .Where(m => ids.Contains(m.ApplicationId)
                && m.SenderUserId != userId
                && m.ReadAt == null)
            .GroupBy(m => m.ApplicationId)
            .Select(g => new { ApplicationId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ApplicationId, x => x.Count, cancellationToken);

        return applications.Select(a =>
        {
            lastMessages.TryGetValue(a.Id, out var last);
            unreadCounts.TryGetValue(a.Id, out var unread);
            var preview = last?.Content;
            if (!string.IsNullOrEmpty(preview) && preview.Length > 80)
            {
                preview = preview[..80] + "…";
            }

            return new ChatThreadDto
            {
                ApplicationId = a.Id,
                PartnerUserId = a.PartnerUserId,
                Title = a.Title,
                Subtitle = a.Subtitle,
                PartnerIsOnline = _presence.IsUserOnline(a.PartnerUserId),
                UnreadCount = unread,
                LastMessagePreview = preview,
                LastMessageAt = last?.SentAt
            };
        }).ToList();
    }

    private static IReadOnlyList<ChatThreadDto> OrderThreads(IReadOnlyList<ChatThreadDto> threads) =>
        threads
            .OrderByDescending(t => t.UnreadCount > 0)
            .ThenByDescending(t => t.LastMessageAt ?? DateTime.MinValue)
            .ThenByDescending(t => t.ApplicationId)
            .ToList();

    private async Task<ApplicationAccess> EnsureCanAccessApplicationAsync(
        long userId,
        string role,
        long applicationId,
        CancellationToken cancellationToken)
    {
        var application = await _context.Applications
            .AsNoTracking()
            .Include(a => a.Job)
            .ThenInclude(j => j.Employer)
            .Include(a => a.JobSeeker)
            .FirstOrDefaultAsync(a => a.Id == applicationId, cancellationToken);

        if (application == null)
        {
            throw new NotFoundException($"Application with id {applicationId} was not found.");
        }

        var normalizedRole = NormalizeRole(role);

        if (normalizedRole == "EMPLOYER")
        {
            if (application.Job.Employer.UserId != userId)
            {
                throw new ForbiddenException("Bạn không có quyền truy cập hội thoại này.");
            }

            return new ApplicationAccess(
                application.Job.Title,
                application.JobSeeker.Name,
                application.JobSeeker.UserId);
        }

        if (normalizedRole == "JOB_SEEKER")
        {
            if (application.JobSeeker.UserId != userId)
            {
                throw new ForbiddenException("Bạn không có quyền truy cập hội thoại này.");
            }

            return new ApplicationAccess(
                application.Job.Title,
                application.Job.Employer.Name,
                application.Job.Employer.UserId);
        }

        throw new ForbiddenException("Chỉ nhà tuyển dụng hoặc ứng viên mới được dùng tin nhắn.");
    }

    private static ChatMessageDto MapMessage(ChatMessage message, long currentUserId) => new()
    {
        Id = message.Id,
        ApplicationId = message.ApplicationId,
        SenderUserId = message.SenderUserId,
        SenderName = message.Sender.Name,
        Content = message.Content,
        SentAt = message.SentAt,
        ReadAt = message.ReadAt,
        IsMine = message.SenderUserId == currentUserId
    };

    private static string NormalizeRole(string role) => role.Trim().ToUpperInvariant();

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

    private async Task<long> GetJobSeekerIdForUserAsync(long userId, CancellationToken cancellationToken)
    {
        var seekerId = await _context.JobSeekers
            .AsNoTracking()
            .Where(s => s.UserId == userId)
            .Select(s => s.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (seekerId == 0)
        {
            throw new NotFoundException("Job seeker profile not found for this user.");
        }

        return seekerId;
    }

    private sealed record ApplicationAccess(string JobTitle, string PartnerName, long PartnerUserId);
}
