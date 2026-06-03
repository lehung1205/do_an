using AutoMapper;
using JobPortal.API.Data;
using JobPortal.API.DTOs;
using JobPortal.API.DTOs.Common;
using JobPortal.API.Exceptions;
using JobPortal.API.Models;
using JobPortal.API.Repositories.Interface;
using JobPortal.API.Services.Interface;
using Microsoft.EntityFrameworkCore;


namespace JobPortal.API.Services.Implementation;

public class PaymentHistoryService : IPaymentHistoryService
{
    private readonly IPaymentHistoryRepository _repository;
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public PaymentHistoryService(IPaymentHistoryRepository repository, AppDbContext context, IMapper mapper)
    {
        _repository = repository;
        _context = context;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<PaymentHistoryDto>> GetAllPaymentHistoriesAsync(CancellationToken cancellationToken = default)
    {
        var items = await _repository.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<PaymentHistoryDto>>(items);
    }

    public async Task<PaymentHistoryDto> GetPaymentHistoryByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null)
        {
            throw new NotFoundException($"Payment history with id {id} was not found.");
        }

        return _mapper.Map<PaymentHistoryDto>(entity);
    }

    public async Task<PaymentHistoryDto> CreatePaymentHistoryAsync(PaymentHistoryDto dto, CancellationToken cancellationToken = default)
    {
        var entity = _mapper.Map<PaymentHistory>(dto);

        if (string.IsNullOrWhiteSpace(entity.OrderId))
        {
            entity.OrderId = Truncate($"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}", 100);
        }

        if (entity.PaymentDate == null &&
            string.Equals(entity.Status, "paid", StringComparison.OrdinalIgnoreCase))
        {
            entity.PaymentDate = DateTime.UtcNow;
        }

        await _repository.AddAsync(entity, cancellationToken);
        return _mapper.Map<PaymentHistoryDto>(entity);
    }

    public async Task<PaymentHistoryDto> CreatePendingPackagePaymentAsync(
        long userId,
        long postingPackageId,
        CancellationToken cancellationToken = default)
    {
        var employer = await _context.Employers
            .FirstOrDefaultAsync(e => e.UserId == userId, cancellationToken);
        if (employer == null)
        {
            throw new NotFoundException("Employer profile was not found for the current user.");
        }

        var package = await _context.PostingPackages
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == postingPackageId && p.IsActive, cancellationToken);
        if (package == null)
        {
            throw new NotFoundException($"Active posting package with id {postingPackageId} was not found.");
        }

        var now = DateTime.UtcNow;
        var orderId = Truncate($"VNP-{now:yyyyMMddHHmmss}-{Guid.NewGuid():N}", 100);
        var payment = new PaymentHistory
        {
            EmployerId = employer.Id,
            PostingPackageId = package.Id,
            Amount = package.Price,
            Currency = "VND",
            OrderId = orderId,
            Status = "pending",
            PaymentProvider = "VNPay",
            PackageNameSnapshot = package.Name,
            PriceSnapshot = package.Price,
            PostingLimitSnapshot = package.PostingLimit,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _repository.AddAsync(payment, cancellationToken);
        return _mapper.Map<PaymentHistoryDto>(payment);
    }

    public async Task<VnPayPaymentResult> ConfirmVnPayPaymentAsync(
        long paymentHistoryId,
        bool isSuccessful,
        string? providerTransactionId,
        string? bankCode,
        string? bankTransactionCode,
        string? responseCode,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        var payment = await _context.PaymentHistories
            .Include(p => p.Employer)
            .Include(p => p.PostingPackage)
            .FirstOrDefaultAsync(p => p.Id == paymentHistoryId, cancellationToken);

        if (payment == null)
        {
            throw new NotFoundException($"Payment history with id {paymentHistoryId} was not found.");
        }

        if (string.Equals(payment.Status, "paid", StringComparison.OrdinalIgnoreCase))
        {
            return new VnPayPaymentResult
            {
                Success = true,
                Message = "Đơn hàng đã được thanh toán trước đó.",
                PaymentHistoryId = payment.Id,
                Amount = payment.Amount,
                TransactionId = payment.ProviderTransactionId,
                ResponseCode = responseCode
            };
        }

        var now = DateTime.UtcNow;
        payment.PaymentBank = string.IsNullOrWhiteSpace(bankCode) ? payment.PaymentBank : bankCode;
        payment.ProviderTransactionId = string.IsNullOrWhiteSpace(providerTransactionId)
            ? payment.ProviderTransactionId
            : providerTransactionId;
        payment.TransactionCode = string.IsNullOrWhiteSpace(bankTransactionCode)
            ? payment.TransactionCode
            : bankTransactionCode;
        payment.UpdatedAt = now;

        if (isSuccessful)
        {
            payment.Status = "paid";
            payment.PaymentDate = now;
            payment.ExpiredAt = now.AddMonths(1);
            payment.Employer.PostingLimit += payment.PostingLimitSnapshot ?? payment.PostingPackage.PostingLimit;
        }
        else
        {
            payment.Status = "failed";
        }

        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new VnPayPaymentResult
        {
            Success = isSuccessful,
            Message = isSuccessful ? "Thanh toán thành công." : "Thanh toán thất bại.",
            PaymentHistoryId = payment.Id,
            Amount = payment.Amount,
            TransactionId = payment.ProviderTransactionId,
            ResponseCode = responseCode
        };
    }

    public async Task UpdatePaymentHistoryAsync(long id, PaymentHistoryDto dto, CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing == null)
        {
            throw new NotFoundException($"Payment history with id {id} was not found.");
        }

        _mapper.Map(dto, existing);
        existing.Id = id;
        await _repository.UpdateAsync(existing, cancellationToken);
    }

    public async Task DeletePaymentHistoryAsync(long id, CancellationToken cancellationToken = default)
    {
        var deleted = await _repository.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            throw new NotFoundException($"Payment history with id {id} was not found.");
        }
    }

    public async Task<EmployerPaymentHistoryResultDto> GetEmployerPaymentHistoryForUserAsync(
        long userId,
        int page = 1,
        int pageSize = 10,
        string? status = null,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
        {
            page = 1;
        }

        if (pageSize < 1)
        {
            pageSize = 10;
        }

        pageSize = Math.Min(pageSize, 50);

        var employer = await _context.Employers
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.UserId == userId, cancellationToken);

        if (employer == null)
        {
            throw new NotFoundException("Employer profile was not found for the current user.");
        }

        var allForEmployer = await _context.PaymentHistories
            .AsNoTracking()
            .Where(p => p.EmployerId == employer.Id)
            .Select(p => new { p.Amount, p.Status })
            .ToListAsync(cancellationToken);

        static bool IsPaid(string s) => string.Equals(s, "paid", StringComparison.OrdinalIgnoreCase);
        static bool IsPending(string s) => string.Equals(s, "pending", StringComparison.OrdinalIgnoreCase);
        static bool IsFailed(string s) => string.Equals(s, "failed", StringComparison.OrdinalIgnoreCase);

        var summary = new EmployerPaymentSummaryDto
        {
            TotalPaidAmount = allForEmployer.Where(p => IsPaid(p.Status)).Sum(p => (long)p.Amount),
            PaidCount = allForEmployer.Count(p => IsPaid(p.Status)),
            PendingCount = allForEmployer.Count(p => IsPending(p.Status)),
            FailedCount = allForEmployer.Count(p => IsFailed(p.Status)),
            TotalCount = allForEmployer.Count,
            CurrentPostingLimit = employer.PostingLimit
        };

        var query = _context.PaymentHistories
            .AsNoTracking()
            .Include(p => p.PostingPackage)
            .Where(p => p.EmployerId == employer.Id);

        if (!string.IsNullOrWhiteSpace(status) && !string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
        {
            var st = status.Trim().ToLowerInvariant();
            query = query.Where(p => p.Status.ToLower() == st);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(p =>
                p.OrderId.Contains(term) ||
                (p.PackageNameSnapshot != null && p.PackageNameSnapshot.Contains(term)) ||
                (p.ProviderTransactionId != null && p.ProviderTransactionId.Contains(term)) ||
                (p.TransactionCode != null && p.TransactionCode.Contains(term)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .ThenByDescending(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new EmployerPaymentListItemDto
            {
                Id = p.Id,
                PackageName = p.PackageNameSnapshot ?? p.PostingPackage.Name,
                PostingLimitSnapshot = p.PostingLimitSnapshot,
                Amount = p.Amount,
                Currency = p.Currency,
                OrderId = p.OrderId,
                Status = p.Status,
                PaymentProvider = p.PaymentProvider,
                PaymentBank = p.PaymentBank,
                ProviderTransactionId = p.ProviderTransactionId,
                TransactionCode = p.TransactionCode,
                PaymentDate = p.PaymentDate,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new EmployerPaymentHistoryResultDto
        {
            Summary = summary,
            Payments = new PagedResult<EmployerPaymentListItemDto>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize)
            }
        };
    }

    private static string Truncate(string value, int maxLength)
    {
        return value.Length <= maxLength ? value : value[..maxLength];
    }
}
