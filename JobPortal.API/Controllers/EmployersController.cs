using JobPortal.API.DTOs;
using JobPortal.API.DTOs.Common;
using JobPortal.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace JobPortal.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class EmployersController : ControllerBase
{
    private readonly IEmployerService _employerService;

    public EmployersController(IEmployerService employerService)
    {
        _employerService = employerService;
    }

    [HttpGet]
    public async Task<IActionResult> GetEmployers(CancellationToken cancellationToken)
    {
        var items = await _employerService.GetAllEmployersAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<EmployerDto>>.SuccessResponse(items, "Employers retrieved successfully."));
    }

    /// <summary>Tất cả nhà tuyển dụng kèm số sao trung bình (đánh giá từ ứng viên).</summary>
    [HttpGet("with-rating")]
    public async Task<IActionResult> GetAllEmployersWithRating(CancellationToken cancellationToken)
    {
        var items = await _employerService.GetAllEmployersWithRatingAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<EmployerWithRatingDto>>.SuccessResponse(
            items,
            "Employers with rating retrieved successfully."));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetEmployer(long id, CancellationToken cancellationToken)
    {
        var item = await _employerService.GetEmployerByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<EmployerDto>.SuccessResponse(item, "Employer retrieved successfully."));
    }

    [HttpGet("{id:long}/public-profile")]
    public async Task<IActionResult> GetEmployerPublicProfile(long id, CancellationToken cancellationToken)
    {
        var profile = await _employerService.GetEmployerPublicProfileAsync(id, cancellationToken);
        return Ok(ApiResponse<EmployerPublicProfileDto>.SuccessResponse(
            profile,
            "Employer public profile retrieved successfully."));
    }

    [HttpPost]
    public async Task<IActionResult> CreateEmployer([FromBody] CreateEmployerDto dto, CancellationToken cancellationToken)
    {
        var created = await _employerService.CreateEmployerAsync(dto, cancellationToken);
        return CreatedAtAction(
            nameof(GetEmployer),
            new { id = created.Id },
            ApiResponse<EmployerDto>.SuccessResponse(created, "Employer created successfully."));
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> UpdateEmployer(long id, [FromBody] UpdateEmployerDto dto, CancellationToken cancellationToken)
    {
        await _employerService.UpdateEmployerAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Employer updated successfully."));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeleteEmployer(long id, CancellationToken cancellationToken)
    {
        await _employerService.DeleteEmployerAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Employer deleted successfully."));
    }
}
