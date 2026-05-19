using JobPortal.API.DTOs;
using JobPortal.API.DTOs.Common;
using JobPortal.API.Services;
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
}
