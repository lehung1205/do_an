using JobPortal.API.DTOs;
using JobPortal.API.DTOs.Common;
using JobPortal.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace JobPortal.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AdminsController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminsController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAdmins(CancellationToken cancellationToken)
    {
        var items = await _adminService.GetAllAdminsAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<AdminDto>>.SuccessResponse(items, "Admins retrieved successfully."));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetAdmin(long id, CancellationToken cancellationToken)
    {
        var item = await _adminService.GetAdminByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<AdminDto>.SuccessResponse(item, "Admin retrieved successfully."));
    }

    [HttpPost]
    public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminDto dto, CancellationToken cancellationToken)
    {
        var created = await _adminService.CreateAdminAsync(dto, cancellationToken);
        return CreatedAtAction(
            nameof(GetAdmin),
            new { id = created.Id },
            ApiResponse<AdminDto>.SuccessResponse(created, "Admin created successfully."));
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> UpdateAdmin(long id, [FromBody] UpdateAdminDto dto, CancellationToken cancellationToken)
    {
        await _adminService.UpdateAdminAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Admin updated successfully."));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeleteAdmin(long id, CancellationToken cancellationToken)
    {
        await _adminService.DeleteAdminAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Admin deleted successfully."));
    }
}
