using JobPortal.API.DTOs;
using JobPortal.API.DTOs.Common;

namespace JobPortal.API.Services.Interface;

public interface IChatService
{
    Task<IReadOnlyList<ChatThreadDto>> GetThreadsForUserAsync(long userId, string role, CancellationToken cancellationToken = default);

    Task<PagedResult<ChatMessageDto>> GetMessagesAsync(
        long userId,
        string role,
        long applicationId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<ChatMessageDto> SendMessageAsync(
        long userId,
        string role,
        long applicationId,
        string content,
        CancellationToken cancellationToken = default);

    Task MarkAsReadAsync(
        long userId,
        string role,
        long applicationId,
        CancellationToken cancellationToken = default);

    Task<ChatJoinedDto> GetThreadInfoAsync(
        long userId,
        string role,
        long applicationId,
        CancellationToken cancellationToken = default);

    Task<int> GetTotalUnreadCountAsync(long userId, string role, CancellationToken cancellationToken = default);
}
