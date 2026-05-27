using JobPortal.API.Data;
using JobPortal.API.DTOs;
using JobPortal.API.Exceptions;
using JobPortal.API.Models;
using JobPortal.API.Repositories.Interface;
using JobPortal.API.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.API.Services.Implementation;

public class AdminPostingPackageService : IAdminPostingPackageService
{
    private readonly IPostingPackageRepository _repository;
    private readonly AppDbContext _context;

    public AdminPostingPackageService(IPostingPackageRepository repository, AppDbContext context)
    {
        _repository = repository;
        _context = context;
    }

    public async Task<IReadOnlyList<AdminPostingPackageDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await _repository.GetAllWithPaymentCountsAsync(cancellationToken);
        return items.Select(MapToDto).ToList();
    }

    public async Task<AdminPostingPackageDto> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdWithPaymentCountAsync(id, cancellationToken);
        if (entity == null)
        {
            throw new NotFoundException($"Posting package with id {id} was not found.");
        }

        return MapToDto(entity.Value.Package, entity.Value.PaymentCount);
    }

    public async Task<AdminPostingPackageDto> CreateAsync(
        long adminUserId,
        CreateAdminPostingPackageRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request.Name, request.Price, request.PostingLimit);
        await EnsureNameAvailableAsync(request.Name.Trim(), null, cancellationToken);

        var adminId = await ResolveAdminIdAsync(adminUserId, cancellationToken);
        var now = DateTime.UtcNow;
        var entity = new PostingPackage
        {
            AdminId = adminId,
            Name = request.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            Price = request.Price,
            PostingLimit = request.PostingLimit,
            IsActive = request.IsActive,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _repository.AddAsync(entity, cancellationToken);
        return MapToDto(entity, 0);
    }

    public async Task<AdminPostingPackageDto> UpdateAsync(
        long id,
        UpdateAdminPostingPackageRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request.Name, request.Price, request.PostingLimit);
        await EnsureNameAvailableAsync(request.Name.Trim(), id, cancellationToken);

        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing == null)
        {
            throw new NotFoundException($"Posting package with id {id} was not found.");
        }

        existing.Name = request.Name.Trim();
        existing.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        existing.Price = request.Price;
        existing.PostingLimit = request.PostingLimit;
        existing.IsActive = request.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(existing, cancellationToken);
        var paymentCount = await _repository.GetPaymentCountAsync(id, cancellationToken);
        return MapToDto(existing, paymentCount);
    }

    public async Task<AdminPostingPackageDto> SetActiveAsync(long id, bool isActive, CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing == null)
        {
            throw new NotFoundException($"Posting package with id {id} was not found.");
        }

        existing.IsActive = isActive;
        existing.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(existing, cancellationToken);

        var paymentCount = await _repository.GetPaymentCountAsync(id, cancellationToken);
        return MapToDto(existing, paymentCount);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing == null)
        {
            throw new NotFoundException($"Posting package with id {id} was not found.");
        }

        var paymentCount = await _repository.GetPaymentCountAsync(id, cancellationToken);
        if (paymentCount > 0)
        {
            throw new BadRequestException(
                "Không thể xóa gói đã có giao dịch. Hãy tắt trạng thái hoạt động để ngừng bán.");
        }

        var deleted = await _repository.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            throw new NotFoundException($"Posting package with id {id} was not found.");
        }
    }

    private static void ValidateRequest(string name, int price, int postingLimit)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new BadRequestException("Tên gói không được để trống.");
        }

        if (name.Trim().Length > 255)
        {
            throw new BadRequestException("Tên gói tối đa 255 ký tự.");
        }

        if (price <= 0)
        {
            throw new BadRequestException("Giá gói phải lớn hơn 0.");
        }

        if (postingLimit <= 0)
        {
            throw new BadRequestException("Số lượt đăng tin phải lớn hơn 0.");
        }
    }

    private async Task EnsureNameAvailableAsync(string name, long? excludeId, CancellationToken cancellationToken)
    {
        var exists = await _repository.ExistsByNameAsync(name, excludeId, cancellationToken);
        if (exists)
        {
            throw new BadRequestException($"Gói \"{name}\" đã tồn tại. Vui lòng chọn tên khác.");
        }
    }

    private async Task<long> ResolveAdminIdAsync(long adminUserId, CancellationToken cancellationToken)
    {
        var adminId = await _context.Admins
            .AsNoTracking()
            .Where(a => a.UserId == adminUserId)
            .Select(a => a.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (adminId == 0)
        {
            throw new BadRequestException("Không tìm thấy hồ sơ quản trị viên.");
        }

        return adminId;
    }

    private static AdminPostingPackageDto MapToDto(PostingPackage package, int paymentCount) =>
        new()
        {
            Id = package.Id,
            AdminId = package.AdminId,
            Name = package.Name,
            Description = package.Description,
            Price = package.Price,
            PostingLimit = package.PostingLimit,
            IsActive = package.IsActive,
            CreatedAt = package.CreatedAt,
            UpdatedAt = package.UpdatedAt,
            PaymentCount = paymentCount
        };

    private static AdminPostingPackageDto MapToDto((PostingPackage Package, int PaymentCount) item) =>
        MapToDto(item.Package, item.PaymentCount);
}
