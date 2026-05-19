using AutoMapper;
using JobPortal.API.DTOs;
using JobPortal.API.Exceptions;
using JobPortal.API.Models;
using JobPortal.API.Repositories.Interface;
using JobPortal.API.Services.Interface;


namespace JobPortal.API.Services.Implementation;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _repository;
    private readonly IMapper _mapper;

    public ReviewService(IReviewRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<ReviewDto>> GetAllReviewsAsync(CancellationToken cancellationToken = default)
    {
        var items = await _repository.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<ReviewDto>>(items);
    }

    public async Task<ReviewDto> GetReviewByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null)
        {
            throw new NotFoundException($"Review with id {id} was not found.");
        }

        return _mapper.Map<ReviewDto>(entity);
    }

    public async Task<ReviewDto> CreateReviewAsync(ReviewDto dto, CancellationToken cancellationToken = default)
    {
        var entity = _mapper.Map<Review>(dto);
        entity.Comment = entity.Comment?.Trim();
        entity.ReviewType = entity.ReviewType.Trim();
        await _repository.AddAsync(entity, cancellationToken);
        return _mapper.Map<ReviewDto>(entity);
    }

    public async Task UpdateReviewAsync(long id, ReviewDto dto, CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing == null)
        {
            throw new NotFoundException($"Review with id {id} was not found.");
        }

        _mapper.Map(dto, existing);
        existing.Id = id;
        existing.Comment = existing.Comment?.Trim();
        existing.ReviewType = existing.ReviewType.Trim();
        await _repository.UpdateAsync(existing, cancellationToken);
    }

    public async Task DeleteReviewAsync(long id, CancellationToken cancellationToken = default)
    {
        var deleted = await _repository.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            throw new NotFoundException($"Review with id {id} was not found.");
        }
    }
}
