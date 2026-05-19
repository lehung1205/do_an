using JobPortal.API.DTOs;

namespace JobPortal.API.Services.Interface;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryDto>> GetAllCategoriesAsync(CancellationToken cancellationToken = default);
    Task<CategoryDto> GetCategoryByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<CategoryDto> CreateCategoryAsync(CategoryDto dto, CancellationToken cancellationToken = default);
    Task UpdateCategoryAsync(long id, CategoryDto dto, CancellationToken cancellationToken = default);
    Task DeleteCategoryAsync(long id, CancellationToken cancellationToken = default);
}
