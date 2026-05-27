using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using JobPortal.API.DTOs;
using JobPortal.API.DTOs.Common;
using JobPortal.API.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobPortal.API.Controllers;

[Route("api/admin/posting-packages")]
[ApiController]
[Authorize(Roles = "ADMIN")]
public class AdminPostingPackagesController : ControllerBase
{
    private readonly IAdminPostingPackageService _service;

    public AdminPostingPackagesController(IAdminPostingPackageService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var items = await _service.GetAllAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<AdminPostingPackageDto>>.SuccessResponse(
            items,
            "Danh sách gói đăng tin đã được tải."));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id, CancellationToken cancellationToken)
    {
        var item = await _service.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<AdminPostingPackageDto>.SuccessResponse(item, "Gói đăng tin đã được tải."));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateAdminPostingPackageRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var created = await _service.CreateAsync(userId, request, cancellationToken);
        return CreatedAtAction(
            nameof(GetById),
            new { id = created.Id },
            ApiResponse<AdminPostingPackageDto>.SuccessResponse(created, "Đã tạo gói đăng tin."));
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(
        long id,
        [FromBody] UpdateAdminPostingPackageRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await _service.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponse<AdminPostingPackageDto>.SuccessResponse(updated, "Đã cập nhật gói đăng tin."));
    }

    [HttpPost("{id:long}/active")]
    public async Task<IActionResult> SetActive(
        long id,
        [FromBody] SetPostingPackageActiveRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await _service.SetActiveAsync(id, request.IsActive, cancellationToken);
        var message = request.IsActive ? "Đã bật gói đăng tin." : "Đã tắt gói đăng tin.";
        return Ok(ApiResponse<AdminPostingPackageDto>.SuccessResponse(updated, message));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Đã xóa gói đăng tin."));
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
