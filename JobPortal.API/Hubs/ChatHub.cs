using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using JobPortal.API.DTOs;
using JobPortal.API.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace JobPortal.API.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IChatService _chatService;
    private readonly IChatPresenceService _presence;

    public ChatHub(IChatService chatService, IChatPresenceService presence)
    {
        _chatService = chatService;
        _presence = presence;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        _presence.RegisterConnection(Context.ConnectionId, userId);
        await Clients.Others.SendAsync("UserPresenceChanged", new ChatPresenceDto
        {
            UserId = userId,
            Online = true
        });
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = _presence.RemoveConnection(Context.ConnectionId);
        if (userId.HasValue && !_presence.IsUserOnline(userId.Value))
        {
            await Clients.Others.SendAsync("UserPresenceChanged", new ChatPresenceDto
            {
                UserId = userId.Value,
                Online = false
            });
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>Mở hội thoại gộp theo đối tác (một room cho mọi đơn ứng tuyển cùng cặp).</summary>
    public async Task JoinChat(long partnerUserId)
    {
        var userId = GetUserId();
        var role = GetUserRole();

        var info = await _chatService.GetThreadInfoByPartnerAsync(userId, role, partnerUserId, Context.ConnectionAborted);
        await _chatService.MarkAsReadByPartnerAsync(userId, role, partnerUserId, Context.ConnectionAborted);

        await Groups.AddToGroupAsync(Context.ConnectionId, PairGroupName(userId, partnerUserId));
        await Clients.Caller.SendAsync("ChatJoined", info);
    }

    /// <summary>Tương thích link cũ theo applicationId — resolve sang đối tác.</summary>
    public async Task JoinChatByApplication(long applicationId)
    {
        var userId = GetUserId();
        var role = GetUserRole();
        var info = await _chatService.GetThreadInfoAsync(userId, role, applicationId, Context.ConnectionAborted);
        await _chatService.MarkAsReadByPartnerAsync(userId, role, info.PartnerUserId, Context.ConnectionAborted);
        await Groups.AddToGroupAsync(Context.ConnectionId, PairGroupName(userId, info.PartnerUserId));
        await Clients.Caller.SendAsync("ChatJoined", info);
    }

    public Task LeaveChat(long partnerUserId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, PairGroupName(GetUserId(), partnerUserId));

    public async Task SendMessage(long partnerUserId, string content)
    {
        var userId = GetUserId();
        var role = GetUserRole();

        var message = await _chatService.SendMessageByPartnerAsync(userId, role, partnerUserId, content, Context.ConnectionAborted);
        var group = PairGroupName(userId, partnerUserId);

        await Clients.OthersInGroup(group).SendAsync("ReceiveMessage", new ChatMessageDto
        {
            Id = message.Id,
            ApplicationId = message.ApplicationId,
            SenderUserId = message.SenderUserId,
            SenderName = message.SenderName,
            Content = message.Content,
            SentAt = message.SentAt,
            ReadAt = message.ReadAt,
            IsMine = false
        });

        await Clients.Caller.SendAsync("ReceiveMessage", message);
    }

    private long GetUserId()
    {
        var sub = Context.User?.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(sub) || !long.TryParse(sub, out var userId))
        {
            throw new HubException("Không xác định được người dùng.");
        }

        return userId;
    }

    private string GetUserRole()
    {
        var role = Context.User?.FindFirstValue(ClaimTypes.Role)
            ?? Context.User?.FindFirstValue("role");
        if (string.IsNullOrWhiteSpace(role))
        {
            throw new HubException("Không xác định được vai trò.");
        }

        return role;
    }

    private static string PairGroupName(long userId, long partnerUserId) =>
        IChatService.GetPairGroupName(userId, partnerUserId);
}
