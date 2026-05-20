using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using JobPortal.API.DTOs;
using JobPortal.API.DTOs.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobPortal.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ResumesController : ControllerBase
{
    private readonly IResumeService _resumeService;

    public ResumesController(IResumeService resumeService)
    {
        _resumeService = resumeService;
    }

    [HttpGet]
    public async Task<IActionResult> GetResumes(CancellationToken cancellationToken)
    {
        var items = await _resumeService.GetAllResumesAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<ResumeDto>>.SuccessResponse(items, "Resumes retrieved successfully."));
    }

    [HttpGet("me")]
    [Authorize(Roles = "JOB_SEEKER")]
    public async Task<IActionResult> GetMyResumes(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var items = await _resumeService.GetResumesForUserAsync(userId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<ResumeDto>>.SuccessResponse(items, "Resumes retrieved successfully."));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetResume(long id, CancellationToken cancellationToken)
    {
        var item = await _resumeService.GetResumeByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<ResumeDto>.SuccessResponse(item, "Resume retrieved successfully."));
    }

    [HttpPost]
    public async Task<IActionResult> CreateResume([FromBody] ResumeDto dto, CancellationToken cancellationToken)
    {
        var created = await _resumeService.CreateResumeAsync(dto, cancellationToken);
        return CreatedAtAction(
            nameof(GetResume),
            new { id = created.Id },
            ApiResponse<ResumeDto>.SuccessResponse(created, "Resume created successfully."));
    }

    [HttpPost("me")]
    [Authorize(Roles = "JOB_SEEKER")]
    public async Task<IActionResult> CreateMyResume([FromBody] CreateResumeRequest request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var created = await _resumeService.CreateResumeForUserAsync(userId, request, cancellationToken);
        return CreatedAtAction(
            nameof(GetResume),
            new { id = created.Id },
            ApiResponse<ResumeDto>.SuccessResponse(created, "Resume created successfully."));
    }

    [HttpDelete("me/{id:long}")]
    [Authorize(Roles = "JOB_SEEKER")]
    public async Task<IActionResult> DeleteMyResume(long id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        await _resumeService.DeleteResumeForUserAsync(userId, id, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Resume deleted successfully."));
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> UpdateResume(long id, [FromBody] ResumeDto dto, CancellationToken cancellationToken)
    {
        await _resumeService.UpdateResumeAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Resume updated successfully."));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeleteResume(long id, CancellationToken cancellationToken)
    {
        await _resumeService.DeleteResumeAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Resume deleted successfully."));
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
