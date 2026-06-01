using AutoMapper;
using JobPortal.API.DTOs;
using JobPortal.API.Exceptions;
using JobPortal.API.Models;
using JobPortal.API.Models.Auth;
using JobPortal.API.Repositories.Interface;
using JobPortal.API.Services.Interface;
using Microsoft.AspNetCore.Identity;


namespace JobPortal.API.Services.Implementation;

public class AdminService : IAdminService
{
    private readonly IAdminRepository _repository;
    private readonly IMapper _mapper;
    private readonly PasswordHasher<User> _passwordHasher;

    public AdminService(IAdminRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
        _passwordHasher = new PasswordHasher<User>();
    }

    public async Task<IReadOnlyList<AdminDto>> GetAllAdminsAsync(CancellationToken cancellationToken = default)
    {
        var items = await _repository.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<AdminDto>>(items);
    }

    public async Task<AdminDto> GetAdminByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null)
        {
            throw new NotFoundException($"Admin with id {id} was not found.");
        }

        return _mapper.Map<AdminDto>(entity);
    }

    public async Task<AdminDto> CreateAdminAsync(CreateAdminDto dto, CancellationToken cancellationToken = default)
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
            Role = "ADMIN",
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = status == "ACTIVE"
        };

        var admin = new Admin
        {
            Name = dto.Name.Trim(),
            AccountNumber = dto.AccountNumber?.Trim(),
            BankName = dto.BankName?.Trim(),
            Status = status,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _repository.AddWithUserAsync(user, admin, cancellationToken);
        return _mapper.Map<AdminDto>(admin);
    }

    public async Task UpdateAdminAsync(long id, UpdateAdminDto dto, CancellationToken cancellationToken = default)
    {
        var admin = await _repository.GetByIdWithUserAsync(id, cancellationToken);
        if (admin == null)
        {
            throw new NotFoundException($"Admin with id {id} was not found.");
        }

        var user = admin.User ?? throw new NotFoundException($"User linked to admin {id} was not found.");

        if (!string.IsNullOrWhiteSpace(dto.Name))
        {
            admin.Name = dto.Name.Trim();
        }

        if (!string.IsNullOrWhiteSpace(dto.Email))
        {
            user.Email = dto.Email.Trim().ToLowerInvariant();
        }

        if (dto.Phone != null)
        {
            user.PhoneNumber = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim();
        }

        if (dto.AccountNumber != null)
        {
            admin.AccountNumber = string.IsNullOrWhiteSpace(dto.AccountNumber) ? null : dto.AccountNumber.Trim();
        }

        if (dto.BankName != null)
        {
            admin.BankName = string.IsNullOrWhiteSpace(dto.BankName) ? null : dto.BankName.Trim();
        }

        if (!string.IsNullOrWhiteSpace(dto.Status))
        {
            admin.Status = dto.Status.Trim().ToUpperInvariant();
        }

        if (!string.IsNullOrWhiteSpace(dto.Role))
        {
            user.Role = dto.Role.Trim().ToUpperInvariant();
        }

        admin.UpdatedAt = DateTime.UtcNow;
        user.IsActive = admin.Status == "ACTIVE";
        user.UpdatedAt = admin.UpdatedAt;

        await _repository.UpdateAsync(admin, user, cancellationToken);
    }

    public async Task DeleteAdminAsync(long id, CancellationToken cancellationToken = default)
    {
        var admin = await _repository.GetByIdWithUserAsync(id, cancellationToken);
        if (admin == null)
        {
            throw new NotFoundException($"Admin with id {id} was not found.");
        }

        await _repository.DeleteWithUserAsync(admin, cancellationToken);
    }
}
