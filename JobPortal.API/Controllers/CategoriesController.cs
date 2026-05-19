using JobPortal.API.DTOs;
using JobPortal.API.DTOs.Common;
using JobPortal.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace JobPortal.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCategories(CancellationToken cancellationToken)
    {
        var items = await _categoryService.GetAllCategoriesAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<CategoryDto>>.SuccessResponse(items, "Categories retrieved successfully."));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetCategory(long id, CancellationToken cancellationToken)
    {
        var item = await _categoryService.GetCategoryByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<CategoryDto>.SuccessResponse(item, "Category retrieved successfully."));
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody] CategoryDto dto, CancellationToken cancellationToken)
    {
        var created = await _categoryService.CreateCategoryAsync(dto, cancellationToken);
        return CreatedAtAction(
            nameof(GetCategory),
            new { id = created.Id },
            ApiResponse<CategoryDto>.SuccessResponse(created, "Category created successfully."));
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> UpdateCategory(long id, [FromBody] CategoryDto dto, CancellationToken cancellationToken)
    {
        await _categoryService.UpdateCategoryAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Category updated successfully."));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeleteCategory(long id, CancellationToken cancellationToken)
    {
        await _categoryService.DeleteCategoryAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Category deleted successfully."));
    }
}
