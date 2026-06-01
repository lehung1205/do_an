using AutoMapper;
using JobPortal.API.Data;
using JobPortal.API.DTOs;
using JobPortal.API.Exceptions;
using JobPortal.API.Helpers;
using JobPortal.API.Models;
using JobPortal.API.Models.Auth;
using JobPortal.API.Repositories.Interface;
using JobPortal.API.Services.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


namespace JobPortal.API.Services.Implementation;

public class EmployerService : IEmployerService
{
    private readonly IEmployerRepository _repository;
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly PasswordHasher<User> _passwordHasher;

    public EmployerService(IEmployerRepository repository, AppDbContext context, IMapper mapper)
    {
        _repository = repository;
        _context = context;
        _mapper = mapper;
        _passwordHasher = new PasswordHasher<User>();
    }

    public async Task<IReadOnlyList<EmployerDto>> GetAllEmployersAsync(CancellationToken cancellationToken = default)
    {
        var items = await _repository.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<EmployerDto>>(items);
    }

    public async Task<EmployerDto> GetEmployerByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null)
        {
            throw new NotFoundException($"Employer with id {id} was not found.");
        }

        return _mapper.Map<EmployerDto>(entity);
    }

    public async Task<EmployerPublicProfileDto> GetEmployerPublicProfileAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null)
        {
            throw new NotFoundException($"Employer with id {id} was not found.");
        }

        var reviewItems = await _context.Reviews
            .AsNoTracking()
            .Where(r =>
                r.EmployerId == id &&
                r.ReviewType == ReviewCatalog.SeekerToEmployer)
            .OrderByDescending(r => r.Id)
            .Select(r => new EmployerReceivedReviewItemDto
            {
                Id = r.Id,
                ApplicationId = r.ApplicationId,
                Rating = r.Rating,
                Comment = r.Comment,
                ApplicantName = r.JobSeeker.Name,
                JobTitle = r.Job.Title
            })
            .ToListAsync(cancellationToken);

        double? averageRating = reviewItems.Count == 0
            ? null
            : Math.Round(reviewItems.Average(i => i.Rating), 1);

        return new EmployerPublicProfileDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            Image = entity.Image,
            Phone = entity.GetPhone(),
            Email = entity.GetEmail(),
            Gender = entity.Gender,
            Reviews = new EmployerReceivedReviewsSummaryDto
            {
                AverageRating = averageRating,
                TotalCount = reviewItems.Count,
                Items = reviewItems
            }
        };
    }

    public async Task<IReadOnlyList<EmployerWithRatingDto>> GetAllEmployersWithRatingAsync(
        CancellationToken cancellationToken = default)
    {
        var employers = await _repository.GetAllAsync(cancellationToken);
        if (employers.Count == 0)
        {
            return Array.Empty<EmployerWithRatingDto>();
        }

        var employerIds = employers.Select(e => e.Id).ToList();
        var ratings = await LoadEmployerRatingsAsync(employerIds, cancellationToken);

        return employers
            .Select(entity =>
            {
                ratings.TryGetValue(entity.Id, out var rating);
                return new EmployerWithRatingDto
                {
                    Id = entity.Id,
                    Name = entity.Name,
                    Description = entity.Description,
                    Image = entity.Image,
                    Phone = entity.GetPhone(),
                    Email = entity.GetEmail(),
                    Gender = entity.Gender,
                    AverageRating = rating?.Average,
                    ReviewCount = rating?.Count ?? 0
                };
            })
            .ToList();
    }

    private async Task<Dictionary<long, EmployerRatingSnapshot>> LoadEmployerRatingsAsync(
        IEnumerable<long> employerIds,
        CancellationToken cancellationToken)
    {
        var ids = employerIds.Distinct().ToList();
        if (ids.Count == 0)
        {
            return new Dictionary<long, EmployerRatingSnapshot>();
        }

        var rows = await _context.Reviews
            .AsNoTracking()
            .Where(r =>
                r.ReviewType == ReviewCatalog.SeekerToEmployer &&
                ids.Contains(r.EmployerId))
            .GroupBy(r => r.EmployerId)
            .Select(g => new
            {
                EmployerId = g.Key,
                Average = g.Average(x => (double)x.Rating),
                Count = g.Count()
            })
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(
            x => x.EmployerId,
            x => new EmployerRatingSnapshot
            {
                Average = Math.Round(x.Average, 1),
                Count = x.Count
            });
    }

    private sealed class EmployerRatingSnapshot
    {
        public double Average { get; init; }
        public int Count { get; init; }
    }

    public async Task<EmployerDto> CreateEmployerAsync(CreateEmployerDto dto, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
        if (await _repository.UserExistsAsync(normalizedEmail, dto.Phone?.Trim(), cancellationToken))
        {
            throw new ConflictException("A user with the provided email or phone already exists.");
        }

        var now = DateTime.UtcNow;
        var passwordHash = _passwordHasher.HashPassword(null!, dto.Password);
        var status = string.IsNullOrWhiteSpace(dto.Status) ? "ACTIVE" : dto.Status.Trim().ToUpperInvariant();

        var user = new User
        {
            Email = normalizedEmail,
            PhoneNumber = dto.Phone?.Trim(),
            PasswordHash = passwordHash,
            Role = "EMPLOYER",
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = status == "ACTIVE"
        };

        var employer = new Employer
        {
            Name = dto.Name.Trim(),
            DateOfBirth = dto.DateOfBirth,
            Gender = dto.Gender,
            Description = dto.Description?.Trim(),
            Image = dto.Image?.Trim(),
            PostingLimit = dto.PostingLimit > 0 ? dto.PostingLimit : 10,
            IdCard = dto.IdCard?.Trim(),
            Status = status,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _repository.AddWithUserAsync(user, employer, cancellationToken);
        return _mapper.Map<EmployerDto>(employer);
    }

    public async Task UpdateEmployerAsync(long id, UpdateEmployerDto dto, CancellationToken cancellationToken = default)
    {
        var employer = await _repository.GetByIdWithUserAsync(id, cancellationToken);
        if (employer == null)
        {
            throw new NotFoundException($"Employer with id {id} was not found.");
        }

        var user = employer.User ?? throw new NotFoundException($"User linked to employer {id} was not found.");

        if (!string.IsNullOrWhiteSpace(dto.Name)) employer.Name = dto.Name.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Email)) user.Email = dto.Email.Trim().ToLowerInvariant();
        if (dto.Phone != null)
        {
            user.PhoneNumber = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim();
        }

        if (dto.EmailVerifiedAt.HasValue) employer.EmailVerifiedAt = dto.EmailVerifiedAt;
        if (dto.DateOfBirth.HasValue) employer.DateOfBirth = dto.DateOfBirth;
        if (dto.Gender.HasValue) employer.Gender = dto.Gender;
        if (dto.Description != null) employer.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
        if (dto.Image != null) employer.Image = string.IsNullOrWhiteSpace(dto.Image) ? null : dto.Image.Trim();
        if (dto.PostingLimit.HasValue) employer.PostingLimit = dto.PostingLimit.Value;
        if (dto.IdCard != null) employer.IdCard = string.IsNullOrWhiteSpace(dto.IdCard) ? null : dto.IdCard.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Status)) employer.Status = dto.Status.Trim().ToUpperInvariant();
        if (!string.IsNullOrWhiteSpace(dto.Role)) user.Role = dto.Role.Trim().ToUpperInvariant();

        employer.UpdatedAt = DateTime.UtcNow;
        user.IsActive = employer.Status == "ACTIVE";
        user.UpdatedAt = employer.UpdatedAt;

        await _repository.UpdateAsync(employer, user, cancellationToken);
    }

    public async Task DeleteEmployerAsync(long id, CancellationToken cancellationToken = default)
    {
        var employer = await _repository.GetByIdWithUserAsync(id, cancellationToken);
        if (employer == null)
        {
            throw new NotFoundException($"Employer with id {id} was not found.");
        }

        await _repository.DeleteWithUserAsync(employer, cancellationToken);
    }
}
