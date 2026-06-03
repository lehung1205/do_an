using JobPortal.API.DTOs;

namespace JobPortal.API.Services.Interface;

public interface INotificationService
{
    Task NotifyJobApprovedAsync(
        long employerUserId,
        long jobId,
        string jobTitle,
        CancellationToken cancellationToken = default);

    Task NotifyJobRejectedAsync(
        long employerUserId,
        long jobId,
        string jobTitle,
        string? reason,
        CancellationToken cancellationToken = default);

    Task NotifyApplicationAcceptedAsync(
        long seekerUserId,
        long applicationId,
        string jobTitle,
        string employerName,
        CancellationToken cancellationToken = default);

    Task NotifyApplicationRejectedAsync(
        long seekerUserId,
        long applicationId,
        string jobTitle,
        string employerName,
        CancellationToken cancellationToken = default);

    Task NotifyWorkProgressUpdatedAsync(
        long seekerUserId,
        long applicationId,
        string jobTitle,
        string employerName,
        string progressTitle,
        string? notes,
        CancellationToken cancellationToken = default);

    Task NotifyNewApplicationAsync(
        long employerUserId,
        long applicationId,
        string applicantName,
        string jobTitle,
        CancellationToken cancellationToken = default);

    Task<int> GetUnreadCountAsync(long userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserNotificationDto>> GetNotificationsAsync(
        long userId,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    Task MarkAsReadAsync(long userId, long notificationId, CancellationToken cancellationToken = default);

    Task MarkAllAsReadAsync(long userId, CancellationToken cancellationToken = default);
}
