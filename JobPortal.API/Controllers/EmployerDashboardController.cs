using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using JobPortal.API.DTOs;
using JobPortal.API.DTOs.Common;
using JobPortal.API.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobPortal.API.Controllers;

[Route("api/employers/me")]
[ApiController]
[Authorize(Roles = "EMPLOYER")]
public class EmployerDashboardController : ControllerBase
{
    private readonly IEmployerDashboardService _dashboardService;

    public EmployerDashboardController(IEmployerDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var dashboard = await _dashboardService.GetDashboardForUserAsync(userId, cancellationToken);
        return Ok(ApiResponse<EmployerDashboardDto>.SuccessResponse(dashboard, "Employer dashboard retrieved successfully."));
    }

    [HttpGet("jobs")]
    public async Task<IActionResult> GetJobs(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var items = await _dashboardService.GetJobsForUserAsync(userId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<EmployerDashboardJobDto>>.SuccessResponse(
            items,
            "Employer jobs retrieved successfully."));
    }

    [HttpGet("applications")]
    public async Task<IActionResult> GetApplications(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var items = await _dashboardService.GetApplicationsForUserAsync(userId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<EmployerDashboardApplicationDto>>.SuccessResponse(
            items,
            "Employer applications retrieved successfully."));
    }

    [HttpPut("applications/{id:long}/status")]
    public async Task<IActionResult> UpdateApplicationStatus(
        long id,
        [FromBody] UpdateEmployerApplicationStatusRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var updated = await _dashboardService.UpdateApplicationStatusAsync(userId, id, request, cancellationToken);
        return Ok(ApiResponse<EmployerDashboardApplicationDto>.SuccessResponse(
            updated,
            "Application status updated successfully."));
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
