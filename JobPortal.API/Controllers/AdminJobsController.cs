using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using JobPortal.API.DTOs;
using JobPortal.API.DTOs.Common;
using JobPortal.API.Helpers;
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

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken = default)
    {
        _ = GetCurrentUserId();
        var summary = await _adminJobService.GetModerationSummaryAsync(cancellationToken);
        return Ok(ApiResponse<AdminJobModerationSummaryDto>.SuccessResponse(
            summary,
            "Job moderation summary retrieved successfully."));
    }

    [HttpGet]
    public async Task<IActionResult> GetJobs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        [FromQuery] string? status = "pending",
        [FromQuery] string? q = null,
        CancellationToken cancellationToken = default)
    {
        _ = GetCurrentUserId();
        var result = await _adminJobService.GetJobsPagedAsync(page, pageSize, status, q, cancellationToken);
        return Ok(ApiResponse<PagedResult<AdminPendingJobDto>>.SuccessResponse(
            result,
            "Jobs retrieved successfully."));
    }

    [HttpGet("by-category/export-excel")]
    public async Task<IActionResult> ExportJobsByCategoryExcel(CancellationToken cancellationToken = default)
    {
        _ = GetCurrentUserId();
        var file = await _adminJobService.ExportJobsByCategoryExcelAsync(cancellationToken);
        return File(file.Content, file.ContentType, file.FileName);
    }

    [HttpGet("export-excel")]
    public async Task<IActionResult> ExportJobsListExcel(
        [FromQuery] string? status = "all",
        [FromQuery] string? q = null,
        CancellationToken cancellationToken = default)
    {
        _ = GetCurrentUserId();
        var file = await _adminJobService.ExportJobsListExcelAsync(status, q, cancellationToken);
        return File(file.Content, file.ContentType, file.FileName);
    }

    [HttpGet("pending")]
    public Task<IActionResult> GetPendingJobs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        [FromQuery] string? q = null,
        CancellationToken cancellationToken = default) =>
        GetJobs(page, pageSize, JobPostingCatalog.Pending, q, cancellationToken);

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
