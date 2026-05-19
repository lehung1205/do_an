using AutoMapper;
using JobPortal.API.DTOs;
using JobPortal.API.Exceptions;
using JobPortal.API.Models;
using JobPortal.API.Repositories;

namespace JobPortal.API.Services;

public class PaymentHistoryService : IPaymentHistoryService
{
    private readonly IPaymentHistoryRepository _repository;
    private readonly IMapper _mapper;

    public PaymentHistoryService(IPaymentHistoryRepository repository, IMapper mapper)
    {
        _repository = repository;
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
            entity.OrderId = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..100];
        }

        if (entity.PaymentDate == null &&
            string.Equals(entity.Status, "paid", StringComparison.OrdinalIgnoreCase))
        {
            entity.PaymentDate = DateTime.UtcNow;
        }

        await _repository.AddAsync(entity, cancellationToken);
        return _mapper.Map<PaymentHistoryDto>(entity);
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
}
