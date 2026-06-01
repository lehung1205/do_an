using JobPortal.API.Data;
using JobPortal.API.DTOs;
using JobPortal.API.DTOs.Common;
using JobPortal.API.Exceptions;
using JobPortal.API.Services.Interface;
using System.Globalization;
using System.Text;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
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
                p.Employer.User.Email.Contains(term) ||
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
                EmployerEmail = p.Employer.User.Email,
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

    public async Task<(byte[] Content, string ContentType, string FileName)> GenerateInvoiceFileAsync(
        long paymentId,
        CancellationToken cancellationToken = default)
    {
        var payment = await _context.PaymentHistories
            .AsNoTracking()
            .Include(p => p.Employer)
            .Include(p => p.PostingPackage)
            .FirstOrDefaultAsync(p => p.Id == paymentId, cancellationToken);

        if (payment == null)
        {
            throw new NotFoundException($"Payment with id {paymentId} was not found.");
        }

        QuestPDF.Settings.License = LicenseType.Community;

        var paidAt = payment.PaymentDate ?? payment.CreatedAt;
        var packageName = payment.PackageNameSnapshot ?? payment.PostingPackage.Name;
        var postingLimit = payment.PostingLimitSnapshot ?? payment.PostingPackage.PostingLimit;
        var amountText = payment.Amount.ToString("N0", CultureInfo.GetCultureInfo("vi-VN"));
        var transactionRef = payment.ProviderTransactionId ?? payment.TransactionCode ?? "N/A";
        var invoiceCode = $"INV-{payment.Id:000000}";

        var bytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(28);
                page.DefaultTextStyle(x => x.FontSize(11).FontColor(Colors.Grey.Darken3));

                page.Header().Column(column =>
                {
                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Column(left =>
                        {
                            left.Item().Text("JOBPORTAL").FontSize(20).Bold().FontColor(Colors.Blue.Medium);
                            left.Item().Text("Nền tảng kết nối việc làm");
                            left.Item().Text("Email: support@jobportal.local");
                        });

                        row.ConstantItem(180).AlignRight().Column(right =>
                        {
                            right.Item().Text("HÓA ĐƠN ĐIỆN TỬ").Bold().FontSize(16);
                            right.Item().Text($"Số hóa đơn: {invoiceCode}");
                            right.Item().Text($"Ngày xuất: {DateTime.UtcNow.ToLocalTime():dd/MM/yyyy HH:mm}");
                        });
                    });

                    column.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                });

                page.Content().PaddingVertical(16).Column(column =>
                {
                    column.Spacing(12);

                    column.Item().Text("Thông tin thanh toán").Bold().FontSize(13).FontColor(Colors.Blue.Darken2);

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(160);
                            columns.RelativeColumn();
                        });

                        void Row(string label, string value)
                        {
                            table.Cell().Background(Colors.Grey.Lighten4).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Text(label).SemiBold();
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Text(value);
                        }

                        Row("Mã đơn hàng", payment.OrderId);
                        Row("Nhà tuyển dụng", payment.Employer.Name);
                        Row("Email", payment.Employer.User.Email);
                        Row("Gói dịch vụ", packageName);
                        Row("Số lượt đăng", postingLimit.ToString(CultureInfo.InvariantCulture));
                        Row("Nhà cung cấp", payment.PaymentProvider ?? "VNPay");
                        Row("Mã tham chiếu", transactionRef);
                        Row("Trạng thái", payment.Status.ToUpperInvariant());
                        Row("Ngày thanh toán", paidAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm"));
                    });

                    column.Item().PaddingTop(8).AlignRight().Column(total =>
                    {
                        total.Item().Text("Tổng thanh toán").SemiBold().FontSize(12);
                        total.Item().Text($"{amountText} {payment.Currency}")
                            .Bold()
                            .FontSize(18)
                            .FontColor(Colors.Green.Darken2);
                    });
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Cảm ơn quý khách đã sử dụng dịch vụ JobPortal. ");
                    x.Span("Tài liệu được tạo từ hệ thống quản trị.").Italic();
                });
            });
        }).GeneratePdf();

        var safeOrderId = SanitizeFileName(payment.OrderId);
        return (bytes, "application/pdf", $"hoa-don-{safeOrderId}.pdf");
    }

    public async Task<(byte[] Content, string ContentType, string FileName)> ExportRevenueExcelAsync(
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.PaymentHistories
            .AsNoTracking()
            .Include(p => p.Employer)
            .Include(p => p.PostingPackage)
            .Where(p => p.Status.ToLower() == "paid");

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

        var rows = await query
            .OrderByDescending(p => p.PaymentDate ?? p.CreatedAt)
            .ThenByDescending(p => p.Id)
            .Select(p => new
            {
                p.OrderId,
                EmployerName = p.Employer.Name,
                EmployerEmail = p.Employer.User.Email,
                PackageName = p.PackageNameSnapshot ?? p.PostingPackage.Name,
                p.Amount,
                p.Currency,
                PaidAt = p.PaymentDate ?? p.CreatedAt,
                p.PaymentProvider,
                p.ProviderTransactionId
            })
            .ToListAsync(cancellationToken);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("DoanhThu");

        worksheet.Cell(1, 1).Value = "BÁO CÁO DOANH THU THANH TOÁN";
        worksheet.Range(1, 1, 1, 9).Merge();
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = 15;
        worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        worksheet.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#E8F0FE");

        var filterText = $"Từ ngày: {(from?.ToString("dd/MM/yyyy") ?? "Tất cả")} - Đến ngày: {(to?.ToString("dd/MM/yyyy") ?? "Tất cả")}";
        worksheet.Cell(2, 1).Value = filterText;
        worksheet.Range(2, 1, 2, 9).Merge();
        worksheet.Cell(2, 1).Style.Font.Italic = true;
        worksheet.Cell(2, 1).Style.Font.FontColor = XLColor.FromHtml("#475569");

        var headers = new[]
        {
            "Mã đơn",
            "Nhà tuyển dụng",
            "Email",
            "Gói dịch vụ",
            "Số tiền",
            "Tiền tệ",
            "Thời gian thanh toán",
            "Cổng thanh toán",
            "Mã giao dịch"
        };

        for (var i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(4, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1D4ED8");
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        var rowIndex = 5;
        long totalRevenue = 0;
        foreach (var row in rows)
        {
            worksheet.Cell(rowIndex, 1).Value = row.OrderId;
            worksheet.Cell(rowIndex, 2).Value = row.EmployerName;
            worksheet.Cell(rowIndex, 3).Value = row.EmployerEmail;
            worksheet.Cell(rowIndex, 4).Value = row.PackageName;
            worksheet.Cell(rowIndex, 5).Value = row.Amount;
            worksheet.Cell(rowIndex, 6).Value = row.Currency;
            worksheet.Cell(rowIndex, 7).Value = row.PaidAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss");
            worksheet.Cell(rowIndex, 8).Value = row.PaymentProvider ?? "VNPay";
            worksheet.Cell(rowIndex, 9).Value = row.ProviderTransactionId ?? string.Empty;

            worksheet.Cell(rowIndex, 5).Style.NumberFormat.Format = "#,##0";
            totalRevenue += row.Amount;
            rowIndex++;
        }

        worksheet.Cell(rowIndex + 1, 1).Value = "Tổng doanh thu (paid)";
        worksheet.Range(rowIndex + 1, 1, rowIndex + 1, 4).Merge();
        worksheet.Cell(rowIndex + 1, 1).Style.Font.Bold = true;
        worksheet.Cell(rowIndex + 1, 5).Value = totalRevenue;
        worksheet.Cell(rowIndex + 1, 5).Style.NumberFormat.Format = "#,##0";
        worksheet.Cell(rowIndex + 1, 5).Style.Font.Bold = true;
        worksheet.Cell(rowIndex + 1, 5).Style.Font.FontColor = XLColor.FromHtml("#166534");
        worksheet.Cell(rowIndex + 1, 6).Value = "VND";

        var tableRange = worksheet.Range(4, 1, Math.Max(4, rowIndex - 1), 9);
        tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        worksheet.Columns(1, 9).AdjustToContents();
        worksheet.SheetView.FreezeRows(4);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var fromPart = from?.ToString("yyyyMMdd") ?? "all";
        var toPart = to?.ToString("yyyyMMdd") ?? "all";
        var fileName = $"doanh-thu-{fromPart}-{toPart}.xlsx";
        return (stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    private static string SanitizeFileName(string value)
    {
        var chars = value.Where(c => !Path.GetInvalidFileNameChars().Contains(c)).ToArray();
        var sanitized = new string(chars).Trim();
        return string.IsNullOrWhiteSpace(sanitized) ? "invoice" : sanitized;
    }
}
