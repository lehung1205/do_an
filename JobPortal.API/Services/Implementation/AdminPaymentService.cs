using JobPortal.API.Data;
using JobPortal.API.DTOs;
using JobPortal.API.DTOs.Common;
using JobPortal.API.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.API.Services.Implementation;

public class AdminPaymentService : IAdminPaymentService
{
    private readonly AppDbContext _context;

    public AdminPaymentService(AppDbContext context) => _context = context;

    public async Task<AdminPaymentRevenueDto> GetRevenueAsync(int months = 6, CancellationToken cancellationToken = default)
    {
        months = Math.Clamp(months, 3, 24);
        var now = DateTime.UtcNow;
        var todayStart = now.Date;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var chartStart = monthStart.AddMonths(-(months - 1));

        var payments = await _context.PaymentHistories
            .AsNoTracking()
            .Select(p => new
            {
                p.Amount,
                p.Status,
                p.PaymentDate,
                p.CreatedAt
            })
            .ToListAsync(cancellationToken);

        static bool IsPaid(string status) =>
            string.Equals(status, "paid", StringComparison.OrdinalIgnoreCase);

        var paid = payments.Where(p => IsPaid(p.Status)).ToList();

        var totalRevenue = paid.Sum(p => (long)p.Amount);
        var monthRevenue = paid
            .Where(p => (p.PaymentDate ?? p.CreatedAt) >= monthStart)
            .Sum(p => (long)p.Amount);
        var todayRevenue = paid
            .Where(p => (p.PaymentDate ?? p.CreatedAt) >= todayStart)
            .Sum(p => (long)p.Amount);

        var byStatus = payments
            .GroupBy(p => p.Status.Trim().ToLowerInvariant())
            .Select(g => new AdminPaymentStatusCountDto
            {
                Status = g.Key,
                Count = g.Count(),
                AmountSum = g.Sum(x => (long)x.Amount)
            })
            .OrderByDescending(x => x.Count)
            .ToList();

        var monthlyRevenue = new List<AdminChartPointDto>();
        for (var i = 0; i < months; i++)
        {
            var start = chartStart.AddMonths(i);
            var end = start.AddMonths(1);
            var label = start.ToString("MM/yyyy");
            var sum = paid
                .Where(p =>
                {
                    var at = p.PaymentDate ?? p.CreatedAt;
                    return at >= start && at < end;
                })
                .Sum(p => (long)p.Amount);
            monthlyRevenue.Add(new AdminChartPointDto { Label = label, Value = (int)Math.Min(sum, int.MaxValue) });
        }

        return new AdminPaymentRevenueDto
        {
            TotalRevenue = totalRevenue,
            MonthRevenue = monthRevenue,
            TodayRevenue = todayRevenue,
            PaidTransactionCount = payments.Count(p => IsPaid(p.Status)),
            PendingTransactionCount = payments.Count(p =>
                string.Equals(p.Status, "pending", StringComparison.OrdinalIgnoreCase)),
            FailedTransactionCount = payments.Count(p =>
                string.Equals(p.Status, "failed", StringComparison.OrdinalIgnoreCase)),
            TotalTransactionCount = payments.Count,
            MonthlyRevenue = monthlyRevenue,
            ByStatus = byStatus
        };
    }

    public async Task<PagedResult<AdminPaymentListItemDto>> GetPaymentHistoryPagedAsync(
        int page,
        int pageSize,
        string? status = null,
        string? search = null,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
        {
            page = 1;
        }

        if (pageSize < 1)
        {
            pageSize = 15;
        }

        pageSize = Math.Min(pageSize, 100);

        var query = _context.PaymentHistories
            .AsNoTracking()
            .Include(p => p.Employer)
            .Include(p => p.PostingPackage)
            .AsQueryable();

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
                p.Employer.Name.Contains(term) ||
                p.Employer.Email.Contains(term) ||
                (p.PackageNameSnapshot != null && p.PackageNameSnapshot.Contains(term)) ||
                (p.ProviderTransactionId != null && p.ProviderTransactionId.Contains(term)) ||
                (p.TransactionCode != null && p.TransactionCode.Contains(term)));
        }

        if (from.HasValue)
        {
            var fromUtc = DateTime.SpecifyKind(from.Value.Date, DateTimeKind.Utc);
            query = query.Where(p => (p.PaymentDate ?? p.CreatedAt) >= fromUtc);
        }

        if (to.HasValue)
        {
            var toExclusive = DateTime.SpecifyKind(to.Value.Date.AddDays(1), DateTimeKind.Utc);
            query = query.Where(p => (p.PaymentDate ?? p.CreatedAt) < toExclusive);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .ThenByDescending(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new AdminPaymentListItemDto
            {
                Id = p.Id,
                EmployerId = p.EmployerId,
                EmployerName = p.Employer.Name,
                EmployerEmail = p.Employer.Email,
                PackageName = p.PackageNameSnapshot ?? p.PostingPackage.Name,
                Amount = p.Amount,
                Currency = p.Currency,
                OrderId = p.OrderId,
                Status = p.Status,
                PaymentProvider = p.PaymentProvider,
                ProviderTransactionId = p.ProviderTransactionId,
                TransactionCode = p.TransactionCode,
                PaymentDate = p.PaymentDate,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<AdminPaymentListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }
}
