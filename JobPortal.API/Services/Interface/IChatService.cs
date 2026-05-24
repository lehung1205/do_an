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

    Task<ChatJoinedDto> GetThreadInfoByPartnerAsync(
        long userId,
        string role,
        long partnerUserId,
        CancellationToken cancellationToken = default);

    Task<PagedResult<ChatMessageDto>> GetMessagesByPartnerAsync(
        long userId,
        string role,
        long partnerUserId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<ChatMessageDto> SendMessageByPartnerAsync(
        long userId,
        string role,
        long partnerUserId,
        string content,
        CancellationToken cancellationToken = default);

    Task MarkAsReadByPartnerAsync(
        long userId,
        string role,
        long partnerUserId,
        CancellationToken cancellationToken = default);

    Task<int> GetTotalUnreadCountAsync(long userId, string role, CancellationToken cancellationToken = default);

    static string GetPairGroupName(long userId, long partnerUserId)
    {
        var a = Math.Min(userId, partnerUserId);
        var b = Math.Max(userId, partnerUserId);
        return $"pair-{a}-{b}";
    }
}
