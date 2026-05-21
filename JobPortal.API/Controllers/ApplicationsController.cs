using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using JobPortal.API.DTOs;
using JobPortal.API.DTOs.Common;
using JobPortal.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobPortal.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ApplicationsController : ControllerBase
{
    private readonly IApplicationService _applicationService;

    public ApplicationsController(IApplicationService applicationService)
    {
        _applicationService = applicationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetApplications(CancellationToken cancellationToken)
    {
        var items = await _applicationService.GetAllApplicationsAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<ApplicationDto>>.SuccessResponse(items, "Applications retrieved successfully."));
    }

    [HttpGet("me")]
    [Authorize(Roles = "JOB_SEEKER")]
    public async Task<IActionResult> GetMyApplications(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var items = await _applicationService.GetMyApplicationsAsync(userId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<MyApplicationDto>>.SuccessResponse(items, "Applications retrieved successfully."));
    }

    [HttpGet("me/job/{jobId:long}/applied")]
    [Authorize(Roles = "JOB_SEEKER")]
    public async Task<IActionResult> HasAppliedToJob(long jobId, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var applied = await _applicationService.HasAppliedToJobAsync(userId, jobId, cancellationToken);
        return Ok(ApiResponse<bool>.SuccessResponse(applied, applied ? "Already applied." : "Not applied yet."));
    }

    [HttpPost("me")]
    [Authorize(Roles = "JOB_SEEKER")]
    public async Task<IActionResult> ApplyForJob([FromBody] CreateApplicationRequest request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var created = await _applicationService.ApplyForJobAsync(userId, request, cancellationToken);
        return Ok(ApiResponse<MyApplicationDto>.SuccessResponse(created, "Application submitted successfully."));
    }

    [HttpGet("me/accepted/work-progress")]
    [Authorize(Roles = "JOB_SEEKER")]
    public async Task<IActionResult> GetMyAcceptedWorkProgressList(
        [FromQuery] string? q,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var items = await _applicationService.GetMyAcceptedWorkProgressListAsync(userId, q, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<SeekerWorkProgressListItemDto>>.SuccessResponse(
            items,
            "Accepted applications work progress list retrieved successfully."));
    }

    [HttpGet("me/{id:long}/work-progress")]
    [Authorize(Roles = "JOB_SEEKER")]
    public async Task<IActionResult> GetMyApplicationWorkProgress(long id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var detail = await _applicationService.GetMyApplicationWorkProgressAsync(userId, id, cancellationToken);
        return Ok(ApiResponse<SeekerApplicationWorkProgressDto>.SuccessResponse(
            detail,
            "Application work progress retrieved successfully."));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetApplication(long id, CancellationToken cancellationToken)
    {
        var item = await _applicationService.GetApplicationByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<ApplicationDto>.SuccessResponse(item, "Application retrieved successfully."));
    }

    [HttpPost]
    public async Task<IActionResult> CreateApplication([FromBody] ApplicationDto dto, CancellationToken cancellationToken)
    {
        var created = await _applicationService.CreateApplicationAsync(dto, cancellationToken);
        return CreatedAtAction(
            nameof(GetApplication),
            new { id = created.Id },
            ApiResponse<ApplicationDto>.SuccessResponse(created, "Application created successfully."));
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> UpdateApplication(long id, [FromBody] ApplicationDto dto, CancellationToken cancellationToken)
    {
        await _applicationService.UpdateApplicationAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Application updated successfully."));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeleteApplication(long id, CancellationToken cancellationToken)
    {
        await _applicationService.DeleteApplicationAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Application deleted successfully."));
    }

    private long GetCurrentUserId()
    {
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!long.TryParse(sub, out var userId))
        {
            throw new UnauthorizedAccessException("User identifier is missing.");
        }

        return userId;
    }
}
