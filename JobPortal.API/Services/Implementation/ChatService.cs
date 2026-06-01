using JobPortal.API.Data;
using JobPortal.API.Helpers;
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

    public Task<PagedResult<ChatMessageDto>> GetMessagesAsync(
        long userId,
        string role,
        long applicationId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return GetMessagesByApplicationEntryAsync(userId, role, applicationId, page, pageSize, cancellationToken);
    }

    public Task<PagedResult<ChatMessageDto>> GetMessagesByPartnerAsync(
        long userId,
        string role,
        long partnerUserId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default) =>
        GetMessagesInternalAsync(userId, role, partnerUserId, page, pageSize, cancellationToken);

    public Task<ChatMessageDto> SendMessageAsync(
        long userId,
        string role,
        long applicationId,
        string content,
        CancellationToken cancellationToken = default)
    {
        return SendMessageByApplicationEntryAsync(userId, role, applicationId, content, cancellationToken);
    }

    public Task<ChatMessageDto> SendMessageByPartnerAsync(
        long userId,
        string role,
        long partnerUserId,
        string content,
        CancellationToken cancellationToken = default) =>
        SendMessageInternalAsync(userId, role, partnerUserId, content, cancellationToken);

    public Task MarkAsReadAsync(
        long userId,
        string role,
        long applicationId,
        CancellationToken cancellationToken = default) =>
        MarkAsReadByApplicationEntryAsync(userId, role, applicationId, cancellationToken);

    public Task MarkAsReadByPartnerAsync(
        long userId,
        string role,
        long partnerUserId,
        CancellationToken cancellationToken = default) =>
        MarkAsReadInternalAsync(userId, role, partnerUserId, cancellationToken);

    public async Task<ChatJoinedDto> GetThreadInfoAsync(
        long userId,
        string role,
        long applicationId,
        CancellationToken cancellationToken = default)
    {
        var access = await EnsureCanAccessApplicationAsync(userId, role, applicationId, cancellationToken);
        return await GetThreadInfoByPartnerAsync(userId, role, access.PartnerUserId, cancellationToken);
    }

    public async Task<ChatJoinedDto> GetThreadInfoByPartnerAsync(
        long userId,
        string role,
        long partnerUserId,
        CancellationToken cancellationToken = default)
    {
        var conversation = await GetConversationByPartnerAsync(userId, role, partnerUserId, cancellationToken);
        return MapJoinedWithPresence(conversation);
    }

    private async Task<PagedResult<ChatMessageDto>> GetMessagesByApplicationEntryAsync(
        long userId,
        string role,
        long applicationId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var access = await EnsureCanAccessApplicationAsync(userId, role, applicationId, cancellationToken);
        return await GetMessagesInternalAsync(userId, role, access.PartnerUserId, page, pageSize, cancellationToken);
    }

    private async Task<PagedResult<ChatMessageDto>> GetMessagesInternalAsync(
        long userId,
        string role,
        long partnerUserId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var conversation = await GetConversationByPartnerAsync(userId, role, partnerUserId, cancellationToken);

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

        var appIds = conversation.ApplicationIds;
        var query = _context.ChatMessages
            .AsNoTracking()
            .Include(m => m.Sender)
                .ThenInclude(u => u!.JobSeekerProfile)
            .Include(m => m.Sender)
                .ThenInclude(u => u!.EmployerProfile)
            .Include(m => m.Sender)
                .ThenInclude(u => u!.AdminProfile)
            .Where(m => appIds.Contains(m.ApplicationId))
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

    private async Task<ChatMessageDto> SendMessageByApplicationEntryAsync(
        long userId,
        string role,
        long applicationId,
        string content,
        CancellationToken cancellationToken)
    {
        var access = await EnsureCanAccessApplicationAsync(userId, role, applicationId, cancellationToken);
        return await SendMessageInternalAsync(userId, role, access.PartnerUserId, content, cancellationToken);
    }

    private async Task<ChatMessageDto> SendMessageInternalAsync(
        long userId,
        string role,
        long partnerUserId,
        string content,
        CancellationToken cancellationToken)
    {
        var conversation = await GetConversationByPartnerAsync(userId, role, partnerUserId, cancellationToken);

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
            ApplicationId = conversation.PrimaryApplicationId,
            SenderUserId = userId,
            Content = trimmed,
            SentAt = DateTime.UtcNow
        };

        _context.ChatMessages.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        await _context.Entry(entity)
            .Reference(m => m.Sender)
            .Query()
            .Include(u => u.JobSeekerProfile)
            .Include(u => u.EmployerProfile)
            .Include(u => u.AdminProfile)
            .LoadAsync(cancellationToken);

        return MapMessage(entity, userId);
    }

    private async Task MarkAsReadByApplicationEntryAsync(
        long userId,
        string role,
        long applicationId,
        CancellationToken cancellationToken)
    {
        var access = await EnsureCanAccessApplicationAsync(userId, role, applicationId, cancellationToken);
        await MarkAsReadInternalAsync(userId, role, access.PartnerUserId, cancellationToken);
    }

    private async Task MarkAsReadInternalAsync(
        long userId,
        string role,
        long partnerUserId,
        CancellationToken cancellationToken)
    {
        var conversation = await GetConversationByPartnerAsync(userId, role, partnerUserId, cancellationToken);
        var appIds = conversation.ApplicationIds;

        var unread = await _context.ChatMessages
            .Where(m => appIds.Contains(m.ApplicationId)
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

        var unreadByApp = await _context.ChatMessages
            .AsNoTracking()
            .Where(m => ids.Contains(m.ApplicationId)
                && m.SenderUserId != userId
                && m.ReadAt == null)
            .GroupBy(m => m.ApplicationId)
            .Select(g => new { ApplicationId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ApplicationId, x => x.Count, cancellationToken);

        return applications
            .GroupBy(a => a.PartnerUserId)
            .Select(g =>
            {
                var ordered = g.OrderByDescending(a => a.AppliedAt).ToList();
                var primary = ordered[0];
                var appIds = ordered.Select(a => a.Id).ToList();

                ChatMessage? last = null;
                foreach (var msg in recentMessages)
                {
                    if (!appIds.Contains(msg.ApplicationId))
                    {
                        continue;
                    }

                    if (last == null || msg.SentAt > last.SentAt)
                    {
                        last = msg;
                    }
                }

                var unread = ordered.Sum(a => unreadByApp.GetValueOrDefault(a.Id));
                var preview = last?.Content;
                if (!string.IsNullOrEmpty(preview) && preview.Length > 80)
                {
                    preview = preview[..80] + "…";
                }

                var subtitle = ordered.Count == 1
                    ? primary.Subtitle
                    : $"{ordered.Count} đơn ứng tuyển";

                return new ChatThreadDto
                {
                    ApplicationId = primary.Id,
                    PartnerUserId = g.Key,
                    Title = primary.Title,
                    Subtitle = subtitle,
                    ApplicationCount = ordered.Count,
                    ApplicationIds = appIds,
                    PartnerIsOnline = _presence.IsUserOnline(g.Key),
                    UnreadCount = unread,
                    LastMessagePreview = preview,
                    LastMessageAt = last?.SentAt
                };
            })
            .ToList();
    }

    private static IReadOnlyList<ChatThreadDto> OrderThreads(IReadOnlyList<ChatThreadDto> threads) =>
        threads
            .OrderByDescending(t => t.UnreadCount > 0)
            .ThenByDescending(t => t.LastMessageAt ?? DateTime.MinValue)
            .ThenByDescending(t => t.PartnerUserId)
            .ToList();

    private async Task<ConversationContext> GetConversationByPartnerAsync(
        long userId,
        string role,
        long partnerUserId,
        CancellationToken cancellationToken)
    {
        if (partnerUserId <= 0)
        {
            throw new BadRequestException("Đối tác chat không hợp lệ.");
        }

        var normalizedRole = NormalizeRole(role);
        IReadOnlyList<ConversationApplication> apps;

        if (normalizedRole == "EMPLOYER")
        {
            var employerId = await GetEmployerIdForUserAsync(userId, cancellationToken);
            apps = await _context.Applications
                .AsNoTracking()
                .Where(a => a.Job.EmployerId == employerId && a.JobSeeker.UserId == partnerUserId)
                .OrderByDescending(a => a.AppliedAt)
                .Select(a => new ConversationApplication(
                    a.Id,
                    a.AppliedAt,
                    a.Job.Title,
                    a.JobSeeker.Name))
                .ToListAsync(cancellationToken);
        }
        else if (normalizedRole == "JOB_SEEKER")
        {
            var seekerId = await GetJobSeekerIdForUserAsync(userId, cancellationToken);
            apps = await _context.Applications
                .AsNoTracking()
                .Where(a => a.JobSeekerId == seekerId && a.Job.Employer.UserId == partnerUserId)
                .OrderByDescending(a => a.AppliedAt)
                .Select(a => new ConversationApplication(
                    a.Id,
                    a.AppliedAt,
                    a.Job.Title,
                    a.Job.Employer.Name))
                .ToListAsync(cancellationToken);
        }
        else
        {
            throw new ForbiddenException("Chỉ nhà tuyển dụng hoặc ứng viên mới được dùng tin nhắn.");
        }

        if (apps.Count == 0)
        {
            throw new ForbiddenException("Bạn không có quyền truy cập hội thoại này.");
        }

        var primary = apps[0];

        return new ConversationContext(
            partnerUserId,
            primary.PartnerName,
            primary.Id,
            apps.Select(a => a.Id).ToList(),
            apps.Count,
            BuildJobTitleSummary(apps));
    }

    private static string BuildJobTitleSummary(IReadOnlyList<ConversationApplication> apps)
    {
        if (apps.Count == 1)
        {
            return apps[0].JobTitle;
        }

        return $"{apps.Count} đơn ứng tuyển · {apps[0].JobTitle}";
    }

    private ChatJoinedDto MapJoinedWithPresence(ConversationContext conversation) => new()
    {
        ApplicationId = conversation.PrimaryApplicationId,
        PartnerUserId = conversation.PartnerUserId,
        PartnerName = conversation.PartnerName,
        JobTitle = conversation.JobTitleSummary,
        ApplicationCount = conversation.ApplicationCount,
        PartnerIsOnline = _presence.IsUserOnline(conversation.PartnerUserId)
    };

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
        SenderName = message.Sender.GetDisplayName(),
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

    private sealed record ConversationApplication(long Id, DateTime AppliedAt, string JobTitle, string PartnerName);

    private sealed record ConversationContext(
        long PartnerUserId,
        string PartnerName,
        long PrimaryApplicationId,
        IReadOnlyList<long> ApplicationIds,
        int ApplicationCount,
        string JobTitleSummary);
}
