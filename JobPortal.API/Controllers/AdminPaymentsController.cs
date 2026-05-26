using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using JobPortal.API.DTOs.Common;
using JobPortal.API.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobPortal.API.Controllers;

[Route("api/admin/payments")]
[ApiController]
[Authorize(Roles = "ADMIN")]
public class AdminPaymentsController : ControllerBase
{
    private readonly IAdminPaymentService _paymentService;

    public AdminPaymentsController(IAdminPaymentService paymentService) =>
        _paymentService = paymentService;

    [HttpGet("revenue")]
    public async Task<IActionResult> GetRevenue([FromQuery] int months = 6, CancellationToken cancellationToken = default)
    {
        _ = GetCurrentUserId();
        var data = await _paymentService.GetRevenueAsync(months, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(data, "Payment revenue retrieved successfully."));
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 15,
        [FromQuery] string? status = null,
        [FromQuery] string? q = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        _ = GetCurrentUserId();
        var result = await _paymentService.GetPaymentHistoryPagedAsync(
            page, pageSize, status, q, from, to, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(result, "Payment history retrieved successfully."));
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
}
