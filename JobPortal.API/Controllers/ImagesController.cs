using JobPortal.API.DTOs;
using JobPortal.API.DTOs.Common;
using JobPortal.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace JobPortal.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ImagesController : ControllerBase
{
    private readonly IImageService _imageService;

    public ImagesController(IImageService imageService)
    {
        _imageService = imageService;
    }

    [HttpGet]
    public async Task<IActionResult> GetImages(CancellationToken cancellationToken)
    {
        var items = await _imageService.GetAllImagesAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<ImageDto>>.SuccessResponse(items, "Images retrieved successfully."));
    }

    [HttpGet("job/{jobId:long}")]
    public async Task<IActionResult> GetImagesByJob(long jobId, CancellationToken cancellationToken)
    {
        var items = await _imageService.GetImagesByJobIdAsync(jobId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<ImageDto>>.SuccessResponse(items, "Images retrieved successfully."));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetImage(long id, CancellationToken cancellationToken)
    {
        var item = await _imageService.GetImageByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<ImageDto>.SuccessResponse(item, "Image retrieved successfully."));
    }

    [HttpPost]
    public async Task<IActionResult> CreateImage([FromBody] ImageDto dto, CancellationToken cancellationToken)
    {
        var created = await _imageService.CreateImageAsync(dto, cancellationToken);
        return CreatedAtAction(
            nameof(GetImage),
            new { id = created.Id },
            ApiResponse<ImageDto>.SuccessResponse(created, "Image created successfully."));
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> UpdateImage(long id, [FromBody] ImageDto dto, CancellationToken cancellationToken)
    {
        await _imageService.UpdateImageAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Image updated successfully."));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeleteImage(long id, CancellationToken cancellationToken)
    {
        await _imageService.DeleteImageAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Image deleted successfully."));
    }
}
