using AutoMapper;
using JobPortal.API.DTOs;
using JobPortal.API.Exceptions;
using JobPortal.API.Models;
using JobPortal.API.Models.Auth;
using JobPortal.API.Repositories.Interface;
using JobPortal.API.Services.Interface;
using Microsoft.AspNetCore.Identity;


namespace JobPortal.API.Services.Implementation;

public class JobSeekerService : IJobSeekerService
{
    private readonly IJobSeekerRepository _repository;
    private readonly IMapper _mapper;
    private readonly PasswordHasher<User> _passwordHasher;

    public JobSeekerService(IJobSeekerRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
        _passwordHasher = new PasswordHasher<User>();
    }

    public async Task<IReadOnlyList<JobSeekerDto>> GetAllJobSeekersAsync(CancellationToken cancellationToken = default)
    {
        var items = await _repository.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<JobSeekerDto>>(items);
    }

    public async Task<JobSeekerDto> GetJobSeekerByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null)
        {
            throw new NotFoundException($"Job seeker with id {id} was not found.");
        }

        return _mapper.Map<JobSeekerDto>(entity);
    }

    public async Task<JobSeekerDto> CreateJobSeekerAsync(CreateJobSeekerDto dto, CancellationToken cancellationToken = default)
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
            Role = "JOB_SEEKER",
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = status == "ACTIVE"
        };

        var jobSeeker = new JobSeeker
        {
            Name = dto.Name.Trim(),
            DateOfBirth = dto.DateOfBirth,
            Gender = dto.Gender,
            Description = dto.Description?.Trim(),
            ProfileImage = dto.ProfileImage?.Trim(),
            IdCard = dto.IdCard?.Trim(),
            IdCardIssueDate = dto.IdCardIssueDate?.Trim(),
            IdCardIssuePlace = dto.IdCardIssuePlace?.Trim(),
            PermanentAddress = dto.PermanentAddress?.Trim(),
            TemporaryAddress = dto.TemporaryAddress?.Trim(),
            Status = status,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _repository.AddWithUserAsync(user, jobSeeker, cancellationToken);
        return _mapper.Map<JobSeekerDto>(jobSeeker);
    }

    public async Task UpdateJobSeekerAsync(long id, UpdateJobSeekerDto dto, CancellationToken cancellationToken = default)
    {
        var jobSeeker = await _repository.GetByIdWithUserAsync(id, cancellationToken);
        if (jobSeeker == null)
        {
            throw new NotFoundException($"Job seeker with id {id} was not found.");
        }

        var user = jobSeeker.User ?? throw new NotFoundException($"User linked to job seeker {id} was not found.");

        if (!string.IsNullOrWhiteSpace(dto.Name)) jobSeeker.Name = dto.Name.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Email)) user.Email = dto.Email.Trim().ToLowerInvariant();
        if (dto.Phone != null)
        {
            user.PhoneNumber = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim();
        }

        if (dto.EmailVerifiedAt.HasValue) jobSeeker.EmailVerifiedAt = dto.EmailVerifiedAt;
        if (dto.DateOfBirth.HasValue) jobSeeker.DateOfBirth = dto.DateOfBirth;
        if (dto.Gender.HasValue) jobSeeker.Gender = dto.Gender;
        if (dto.Description != null) jobSeeker.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
        if (dto.ProfileImage != null) jobSeeker.ProfileImage = string.IsNullOrWhiteSpace(dto.ProfileImage) ? null : dto.ProfileImage.Trim();
        if (dto.IdCard != null) jobSeeker.IdCard = string.IsNullOrWhiteSpace(dto.IdCard) ? null : dto.IdCard.Trim();
        if (dto.IdCardIssueDate != null) jobSeeker.IdCardIssueDate = string.IsNullOrWhiteSpace(dto.IdCardIssueDate) ? null : dto.IdCardIssueDate.Trim();
        if (dto.IdCardIssuePlace != null) jobSeeker.IdCardIssuePlace = string.IsNullOrWhiteSpace(dto.IdCardIssuePlace) ? null : dto.IdCardIssuePlace.Trim();
        if (dto.PermanentAddress != null) jobSeeker.PermanentAddress = string.IsNullOrWhiteSpace(dto.PermanentAddress) ? null : dto.PermanentAddress.Trim();
        if (dto.TemporaryAddress != null) jobSeeker.TemporaryAddress = string.IsNullOrWhiteSpace(dto.TemporaryAddress) ? null : dto.TemporaryAddress.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Status)) jobSeeker.Status = dto.Status.Trim().ToUpperInvariant();
        if (!string.IsNullOrWhiteSpace(dto.Role)) user.Role = dto.Role.Trim().ToUpperInvariant();

        jobSeeker.UpdatedAt = DateTime.UtcNow;
        user.IsActive = jobSeeker.Status == "ACTIVE";
        user.UpdatedAt = jobSeeker.UpdatedAt;

        await _repository.UpdateAsync(jobSeeker, user, cancellationToken);
    }

    public async Task DeleteJobSeekerAsync(long id, CancellationToken cancellationToken = default)
    {
        var jobSeeker = await _repository.GetByIdWithUserAsync(id, cancellationToken);
        if (jobSeeker == null)
        {
            throw new NotFoundException($"Job seeker with id {id} was not found.");
        }

        await _repository.DeleteWithUserAsync(jobSeeker, cancellationToken);
    }
}
