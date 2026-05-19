using JobPortal.API.DTOs;
using JobPortal.API.DTOs.Common;
using JobPortal.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace JobPortal.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PostingPackagesController : ControllerBase
{
    private readonly IPostingPackageService _postingPackageService;

    public PostingPackagesController(IPostingPackageService postingPackageService)
    {
        _postingPackageService = postingPackageService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPostingPackages(CancellationToken cancellationToken)
    {
        var packages = await _postingPackageService.GetAllPostingPackagesAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<PostingPackageDto>>.SuccessResponse(packages, "Posting packages retrieved successfully."));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetPostingPackage(long id, CancellationToken cancellationToken)
    {
        var package = await _postingPackageService.GetPostingPackageByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<PostingPackageDto>.SuccessResponse(package, "Posting package retrieved successfully."));
    }

    [HttpPost]
    public async Task<IActionResult> CreatePostingPackage([FromBody] PostingPackageDto dto, CancellationToken cancellationToken)
    {
        var created = await _postingPackageService.CreatePostingPackageAsync(dto, cancellationToken);
        return CreatedAtAction(
            nameof(GetPostingPackage),
            new { id = created.Id },
            ApiResponse<PostingPackageDto>.SuccessResponse(created, "Posting package created successfully."));
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> UpdatePostingPackage(long id, [FromBody] PostingPackageDto dto, CancellationToken cancellationToken)
    {
        await _postingPackageService.UpdatePostingPackageAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Posting package updated successfully."));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeletePostingPackage(long id, CancellationToken cancellationToken)
    {
        await _postingPackageService.DeletePostingPackageAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Posting package deleted successfully."));
    }
}
