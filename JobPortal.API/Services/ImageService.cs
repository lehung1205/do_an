using AutoMapper;
using JobPortal.API.DTOs;
using JobPortal.API.Exceptions;
using JobPortal.API.Models;
using JobPortal.API.Repositories;

namespace JobPortal.API.Services;

public class ImageService : IImageService
{
    private readonly IImageRepository _repository;
    private readonly IMapper _mapper;

    public ImageService(IImageRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<ImageDto>> GetAllImagesAsync(CancellationToken cancellationToken = default)
    {
        var items = await _repository.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<ImageDto>>(items);
    }

    public async Task<ImageDto> GetImageByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null)
        {
            throw new NotFoundException($"Image with id {id} was not found.");
        }

        return _mapper.Map<ImageDto>(entity);
    }

    public async Task<ImageDto> CreateImageAsync(ImageDto dto, CancellationToken cancellationToken = default)
    {
        var entity = _mapper.Map<Image>(dto);
        entity.Url = entity.Url.Trim();
        entity.Name = entity.Name?.Trim();
        await _repository.AddAsync(entity, cancellationToken);
        return _mapper.Map<ImageDto>(entity);
    }

    public async Task UpdateImageAsync(long id, ImageDto dto, CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing == null)
        {
            throw new NotFoundException($"Image with id {id} was not found.");
        }

        _mapper.Map(dto, existing);
        existing.Id = id;
        existing.Url = existing.Url.Trim();
        existing.Name = existing.Name?.Trim();
        await _repository.UpdateAsync(existing, cancellationToken);
    }

    public async Task DeleteImageAsync(long id, CancellationToken cancellationToken = default)
    {
        var deleted = await _repository.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            throw new NotFoundException($"Image with id {id} was not found.");
        }
    }
}
