using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using JobPortal.API.DTOs;
using JobPortal.API.DTOs.Common;
using JobPortal.API.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobPortal.API.Controllers;

[Route("api/chat")]
[ApiController]
[Authorize(Roles = "EMPLOYER,JOB_SEEKER")]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;

    public ChatController(IChatService chatService) => _chatService = chatService;

    [HttpGet("unread-summary")]
    public async Task<IActionResult> GetUnreadSummary(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var role = GetCurrentUserRole();
        var total = await _chatService.GetTotalUnreadCountAsync(userId, role, cancellationToken);
        return Ok(ApiResponse<ChatUnreadSummaryDto>.SuccessResponse(
            new ChatUnreadSummaryDto { TotalUnreadCount = total },
            "Unread summary retrieved."));
    }

    [HttpGet("threads")]
    public async Task<IActionResult> GetThreads(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var role = GetCurrentUserRole();
        var threads = await _chatService.GetThreadsForUserAsync(userId, role, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<ChatThreadDto>>.SuccessResponse(threads, "Chat threads retrieved."));
    }

    [HttpGet("partners/{partnerUserId:long}/messages")]
    public async Task<IActionResult> GetMessagesByPartner(
        long partnerUserId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var role = GetCurrentUserRole();
        var result = await _chatService.GetMessagesByPartnerAsync(userId, role, partnerUserId, page, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResult<ChatMessageDto>>.SuccessResponse(result, "Messages retrieved."));
    }

    [HttpGet("applications/{applicationId:long}/messages")]
    public async Task<IActionResult> GetMessages(
        long applicationId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var role = GetCurrentUserRole();
        var result = await _chatService.GetMessagesAsync(userId, role, applicationId, page, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResult<ChatMessageDto>>.SuccessResponse(result, "Messages retrieved."));
    }

    [HttpPost("partners/{partnerUserId:long}/read")]
    public async Task<IActionResult> MarkReadByPartner(long partnerUserId, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var role = GetCurrentUserRole();
        await _chatService.MarkAsReadByPartnerAsync(userId, role, partnerUserId, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Marked as read."));
    }

    [HttpPost("applications/{applicationId:long}/read")]
    public async Task<IActionResult> MarkRead(long applicationId, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var role = GetCurrentUserRole();
        await _chatService.MarkAsReadAsync(userId, role, applicationId, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Marked as read."));
    }

    private long GetCurrentUserId()
    {
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(sub) || !long.TryParse(sub, out var userId))
        {
            throw new UnauthorizedAccessException("User id claim is missing.");
        }

        return userId;
    }

    private string GetCurrentUserRole()
    {
        var role = User.FindFirstValue(ClaimTypes.Role) ?? User.FindFirstValue("role");
        if (string.IsNullOrWhiteSpace(role))
        {
            throw new UnauthorizedAccessException("Role claim is missing.");
        }

        return role;
    }
}
