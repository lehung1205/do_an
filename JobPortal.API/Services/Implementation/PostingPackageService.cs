using AutoMapper;
using JobPortal.API.DTOs;
using JobPortal.API.Exceptions;
using JobPortal.API.Models;
using JobPortal.API.Repositories.Interface;
using JobPortal.API.Services.Interface;


namespace JobPortal.API.Services.Implementation;

public class PostingPackageService : IPostingPackageService
{
    private readonly IPostingPackageRepository _repository;
    private readonly IMapper _mapper;

    public PostingPackageService(IPostingPackageRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<PostingPackageDto>> GetAllPostingPackagesAsync(CancellationToken cancellationToken = default)
    {
        var items = await _repository.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<PostingPackageDto>>(items);
    }

    public async Task<PostingPackageDto> GetPostingPackageByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null)
        {
            throw new NotFoundException($"Posting package with id {id} was not found.");
        }

        return _mapper.Map<PostingPackageDto>(entity);
    }

    public async Task<PostingPackageDto> CreatePostingPackageAsync(PostingPackageDto dto, CancellationToken cancellationToken = default)
    {
        var entity = _mapper.Map<PostingPackage>(dto);
        await _repository.AddAsync(entity, cancellationToken);
        return _mapper.Map<PostingPackageDto>(entity);
    }

    public async Task UpdatePostingPackageAsync(long id, PostingPackageDto dto, CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing == null)
        {
            throw new NotFoundException($"Posting package with id {id} was not found.");
        }

        _mapper.Map(dto, existing);
        existing.Id = id;
        await _repository.UpdateAsync(existing, cancellationToken);
    }

    public async Task DeletePostingPackageAsync(long id, CancellationToken cancellationToken = default)
    {
        var deleted = await _repository.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            throw new NotFoundException($"Posting package with id {id} was not found.");
        }
    }
}
