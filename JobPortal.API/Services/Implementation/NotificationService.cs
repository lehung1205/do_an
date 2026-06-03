using JobPortal.API.Data;
using JobPortal.API.DTOs;
using JobPortal.API.Exceptions;
using JobPortal.API.Helpers;
using JobPortal.API.Models;
using JobPortal.API.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.API.Services.Implementation;

public class NotificationService : INotificationService
{
    private const int MaxPageSize = 50;
    private readonly AppDbContext _context;

    public NotificationService(AppDbContext context) => _context = context;

    public Task NotifyJobApprovedAsync(
        long employerUserId,
        long jobId,
        string jobTitle,
        CancellationToken cancellationToken = default) =>
        CreateAsync(
            employerUserId,
            NotificationCatalog.JobApproved,
            "Tin tuyển dụng đã được duyệt",
            $"Tin \"{jobTitle}\" đã được quản trị viên phê duyệt và đang hiển thị cho ứng viên.",
            NotificationCatalog.ReferenceJob,
            jobId,
            cancellationToken);

    public Task NotifyJobRejectedAsync(
        long employerUserId,
        long jobId,
        string jobTitle,
        string? reason,
        CancellationToken cancellationToken = default)
    {
        var message = string.IsNullOrWhiteSpace(reason)
            ? $"Tin \"{jobTitle}\" không được duyệt. Bạn có thể chỉnh sửa và đăng lại."
            : $"Tin \"{jobTitle}\" không được duyệt. Lý do: {reason.Trim()}";

        return CreateAsync(
            employerUserId,
            NotificationCatalog.JobRejected,
            "Tin tuyển dụng bị từ chối",
            message,
            NotificationCatalog.ReferenceJob,
            jobId,
            cancellationToken);
    }

    public Task NotifyApplicationAcceptedAsync(
        long seekerUserId,
        long applicationId,
        string jobTitle,
        string employerName,
        CancellationToken cancellationToken = default) =>
        CreateAsync(
            seekerUserId,
            NotificationCatalog.ApplicationAccepted,
            "Đơn ứng tuyển được chấp nhận",
            $"Nhà tuyển dụng {employerName} đã chấp nhận đơn ứng tuyển vị trí \"{jobTitle}\".",
            NotificationCatalog.ReferenceApplication,
            applicationId,
            cancellationToken);

    public Task NotifyApplicationRejectedAsync(
        long seekerUserId,
        long applicationId,
        string jobTitle,
        string employerName,
        CancellationToken cancellationToken = default) =>
        CreateAsync(
            seekerUserId,
            NotificationCatalog.ApplicationRejected,
            "Đơn ứng tuyển bị từ chối",
            $"Nhà tuyển dụng {employerName} đã từ chối đơn ứng tuyển vị trí \"{jobTitle}\".",
            NotificationCatalog.ReferenceApplication,
            applicationId,
            cancellationToken);

    public Task NotifyWorkProgressUpdatedAsync(
        long seekerUserId,
        long applicationId,
        string jobTitle,
        string employerName,
        string progressTitle,
        string? notes,
        CancellationToken cancellationToken = default)
    {
        var message = string.IsNullOrWhiteSpace(notes)
            ? $"{employerName} cập nhật tiến độ công việc \"{jobTitle}\" sang: {progressTitle}."
            : $"{employerName} cập nhật tiến độ \"{jobTitle}\" sang: {progressTitle}. Ghi chú: {notes.Trim()}";

        return CreateAsync(
            seekerUserId,
            NotificationCatalog.WorkProgressUpdated,
            "Tiến độ công việc được cập nhật",
            message,
            NotificationCatalog.ReferenceApplication,
            applicationId,
            cancellationToken);
    }

    public Task NotifyNewApplicationAsync(
        long employerUserId,
        long applicationId,
        string applicantName,
        string jobTitle,
        CancellationToken cancellationToken = default) =>
        CreateAsync(
            employerUserId,
            NotificationCatalog.NewApplication,
            applicantName.Trim(),
            $"Vừa nộp CV cho vị trí \"{jobTitle}\".",
            NotificationCatalog.ReferenceApplication,
            applicationId,
            cancellationToken);

    public Task NotifyReviewReceivedAsync(
        long recipientUserId,
        long applicationId,
        string reviewerName,
        string jobTitle,
        int rating,
        CancellationToken cancellationToken = default) =>
        CreateAsync(
            recipientUserId,
            NotificationCatalog.ReviewReceived,
            "Bạn nhận được đánh giá mới",
            $"{reviewerName.Trim()} đánh giá bạn {rating}/5 sao cho công việc \"{jobTitle}\".",
            NotificationCatalog.ReferenceApplication,
            applicationId,
            cancellationToken);

    public async Task<int> GetUnreadCountAsync(long userId, CancellationToken cancellationToken = default) =>
        await _context.UserNotifications
            .AsNoTracking()
            .CountAsync(n => n.UserId == userId && !n.IsRead, cancellationToken);

    public async Task<IReadOnlyList<UserNotificationDto>> GetNotificationsAsync(
        long userId,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
        {
            page = 1;
        }

        if (pageSize < 1 || pageSize > MaxPageSize)
        {
            pageSize = 20;
        }

        var skip = (page - 1) * pageSize;

        return await _context.UserNotifications
            .AsNoTracking()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ThenByDescending(n => n.Id)
            .Skip(skip)
            .Take(pageSize)
            .Select(n => new UserNotificationDto
            {
                Id = n.Id,
                Type = n.Type,
                Title = n.Title,
                Message = n.Message,
                ReferenceType = n.ReferenceType,
                ReferenceId = n.ReferenceId,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task MarkAsReadAsync(long userId, long notificationId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.UserNotifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException($"Notification with id {notificationId} was not found.");
        }

        if (!entity.IsRead)
        {
            entity.IsRead = true;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task MarkAllAsReadAsync(long userId, CancellationToken cancellationToken = default)
    {
        await _context.UserNotifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true), cancellationToken);
    }

    private async Task CreateAsync(
        long userId,
        string type,
        string title,
        string message,
        string? referenceType,
        long? referenceId,
        CancellationToken cancellationToken)
    {
        _context.UserNotifications.Add(new UserNotification
        {
            UserId = userId,
            Type = type,
            Title = title,
            Message = message,
            ReferenceType = referenceType,
            ReferenceId = referenceId,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);
    }

}
