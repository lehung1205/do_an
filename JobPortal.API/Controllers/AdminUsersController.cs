using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using JobPortal.API.DTOs;
using JobPortal.API.DTOs.Common;
using JobPortal.API.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobPortal.API.Controllers;

[Route("api/admin/users")]
[ApiController]
[Authorize(Roles = "ADMIN")]
public class AdminUsersController : ControllerBase
{
    private readonly IAdminUserManagementService _userManagementService;

    public AdminUsersController(IAdminUserManagementService userManagementService)
    {
        _userManagementService = userManagementService;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken = default)
    {
        _ = GetCurrentUserId();
        var summary = await _userManagementService.GetSummaryAsync(cancellationToken);
        return Ok(ApiResponse<AdminUserManagementSummaryDto>.SuccessResponse(
            summary,
            "User management summary retrieved successfully."));
    }

    [HttpGet("employers")]
    public async Task<IActionResult> GetEmployers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        [FromQuery] string? q = null,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        _ = GetCurrentUserId();
        var result = await _userManagementService.GetEmployersPagedAsync(
            page, pageSize, q, status, cancellationToken);
        return Ok(ApiResponse<PagedResult<AdminManagedEmployerDto>>.SuccessResponse(
            result,
            "Employers retrieved successfully."));
    }

    [HttpGet("job-seekers")]
    public async Task<IActionResult> GetJobSeekers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        [FromQuery] string? q = null,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        _ = GetCurrentUserId();
        var result = await _userManagementService.GetJobSeekersPagedAsync(
            page, pageSize, q, status, cancellationToken);
        return Ok(ApiResponse<PagedResult<AdminManagedJobSeekerDto>>.SuccessResponse(
            result,
            "Job seekers retrieved successfully."));
    }

    [HttpPost("employers/{id:long}/status")]
    public async Task<IActionResult> SetEmployerStatus(
        long id,
        [FromBody] SetAccountActiveRequest request,
        CancellationToken cancellationToken)
    {
        _ = GetCurrentUserId();
        var employer = await _userManagementService.SetEmployerActiveAsync(
            id,
            request.Active,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            cancellationToken);

        var message = request.Active
            ? "Đã kích hoạt tài khoản nhà tuyển dụng."
            : "Đã vô hiệu hóa tài khoản nhà tuyển dụng.";

        return Ok(ApiResponse<AdminManagedEmployerDto>.SuccessResponse(employer, message));
    }

    [HttpPost("job-seekers/{id:long}/status")]
    public async Task<IActionResult> SetJobSeekerStatus(
        long id,
        [FromBody] SetAccountActiveRequest request,
        CancellationToken cancellationToken)
    {
        _ = GetCurrentUserId();
        var jobSeeker = await _userManagementService.SetJobSeekerActiveAsync(
            id,
            request.Active,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            cancellationToken);

        var message = request.Active
            ? "Đã kích hoạt tài khoản ứng viên."
            : "Đã vô hiệu hóa tài khoản ứng viên.";

        return Ok(ApiResponse<AdminManagedJobSeekerDto>.SuccessResponse(jobSeeker, message));
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
