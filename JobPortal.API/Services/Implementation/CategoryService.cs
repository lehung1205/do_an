using AutoMapper;
using JobPortal.API.DTOs;
using JobPortal.API.Exceptions;
using JobPortal.API.Models;
using JobPortal.API.Repositories.Interface;
using JobPortal.API.Services.Interface;


namespace JobPortal.API.Services.Implementation;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repository;
    private readonly IMapper _mapper;

    public CategoryService(ICategoryRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<CategoryDto>> GetAllCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var items = await _repository.GetAllAsync(cancellationToken);
        var dtos = _mapper.Map<List<CategoryDto>>(items);
        return SortOtherCategoryLast(dtos);
    }

    /// <summary>Đưa danh mục "Khác" xuống cuối danh sách (đăng tin, banner trang chủ, …).</summary>
    private static List<CategoryDto> SortOtherCategoryLast(IEnumerable<CategoryDto> categories) =>
        categories
            .OrderBy(c => IsOtherCategory(c.Name) ? 1 : 0)
            .ThenBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

    private static bool IsOtherCategory(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        var trimmed = name.Trim();
        return string.Equals(trimmed, "Khác", StringComparison.OrdinalIgnoreCase)
            || string.Equals(trimmed, "Khac", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<CategoryDto> GetCategoryByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null)
        {
            throw new NotFoundException($"Category with id {id} was not found.");
        }

        return _mapper.Map<CategoryDto>(entity);
    }

    public async Task<CategoryDto> CreateCategoryAsync(CategoryDto dto, CancellationToken cancellationToken = default)
    {
        var entity = _mapper.Map<Category>(dto);
        entity.Name = entity.Name.Trim();
        await _repository.AddAsync(entity, cancellationToken);
        return _mapper.Map<CategoryDto>(entity);
    }

    public async Task UpdateCategoryAsync(long id, CategoryDto dto, CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing == null)
        {
            throw new NotFoundException($"Category with id {id} was not found.");
        }

        _mapper.Map(dto, existing);
        existing.Id = id;
        existing.Name = existing.Name.Trim();
        await _repository.UpdateAsync(existing, cancellationToken);
    }

    public async Task DeleteCategoryAsync(long id, CancellationToken cancellationToken = default)
    {
        var deleted = await _repository.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            throw new NotFoundException($"Category with id {id} was not found.");
        }
    }
}
