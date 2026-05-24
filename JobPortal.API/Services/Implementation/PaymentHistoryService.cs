using AutoMapper;
using JobPortal.API.Data;
using JobPortal.API.DTOs;
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

    private static string Truncate(string value, int maxLength)
    {
        return value.Length <= maxLength ? value : value[..maxLength];
    }
}
