using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using JobPortal.API.DTOs;
using JobPortal.API.DTOs.Common;
using JobPortal.API.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobPortal.API.Controllers;

[Route("api/admin/jobs")]
[ApiController]
[Authorize(Roles = "ADMIN")]
public class AdminJobsController : ControllerBase
{
    private readonly IAdminJobService _adminJobService;

    public AdminJobsController(IAdminJobService adminJobService)
    {
        _adminJobService = adminJobService;
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingJobs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        [FromQuery] string? q = null,
        CancellationToken cancellationToken = default)
    {
        _ = GetCurrentUserId();
        var result = await _adminJobService.GetPendingJobsPagedAsync(page, pageSize, q, cancellationToken);
        return Ok(ApiResponse<PagedResult<AdminPendingJobDto>>.SuccessResponse(
            result,
            "Pending jobs retrieved successfully."));
    }

    [HttpPost("{id:long}/approve")]
    public async Task<IActionResult> ApproveJob(long id, CancellationToken cancellationToken)
    {
        _ = GetCurrentUserId();
        var job = await _adminJobService.ApproveJobAsync(id, cancellationToken);
        return Ok(ApiResponse<JobDto>.SuccessResponse(job, "Đã duyệt tin tuyển dụng."));
    }

    [HttpPost("{id:long}/reject")]
    public async Task<IActionResult> RejectJob(
        long id,
        [FromBody] RejectJobRequest? request,
        CancellationToken cancellationToken)
    {
        _ = GetCurrentUserId();
        var job = await _adminJobService.RejectJobAsync(id, request, cancellationToken);
        return Ok(ApiResponse<JobDto>.SuccessResponse(job, "Đã từ chối tin tuyển dụng."));
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
