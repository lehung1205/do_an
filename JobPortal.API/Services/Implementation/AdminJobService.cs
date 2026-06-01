using ClosedXML.Excel;
using JobPortal.API.Data;
using JobPortal.API.DTOs;
using JobPortal.API.DTOs.Common;
using JobPortal.API.Exceptions;
using JobPortal.API.Helpers;
using JobPortal.API.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.API.Services.Implementation;

public class AdminJobService : IAdminJobService
{
    private const int MaxPageSize = 50;
    private readonly AppDbContext _context;
    private readonly IJobExpiryService _jobExpiryService;
    private readonly INotificationService _notificationService;

    public AdminJobService(
        AppDbContext context,
        IJobExpiryService jobExpiryService,
        INotificationService notificationService)
    {
        _context = context;
        _jobExpiryService = jobExpiryService;
        _notificationService = notificationService;
    }

    public async Task<AdminJobModerationSummaryDto> GetModerationSummaryAsync(
        CancellationToken cancellationToken = default)
    {
        await _jobExpiryService.CloseExpiredJobsAsync(cancellationToken);

        var pending = await _context.Jobs.CountAsync(
            j => j.PostingStatus == JobPostingCatalog.Pending,
            cancellationToken);
        var recruiting = await _context.Jobs.CountAsync(
            j => j.PostingStatus == JobPostingCatalog.Recruiting,
            cancellationToken);
        var rejected = await _context.Jobs.CountAsync(
            j => j.PostingStatus == JobPostingCatalog.Rejected,
            cancellationToken);
        var closed = await _context.Jobs.CountAsync(
            j => j.PostingStatus == JobPostingCatalog.Closed,
            cancellationToken);

        return new AdminJobModerationSummaryDto
        {
            PendingCount = pending,
            RecruitingCount = recruiting,
            RejectedCount = rejected,
            ClosedCount = closed
        };
    }

    public Task<PagedResult<AdminPendingJobDto>> GetJobsPagedAsync(
        int page,
        int pageSize,
        string? status = null,
        string? search = null,
        CancellationToken cancellationToken = default) =>
        GetJobsPagedInternalAsync(page, pageSize, status, search, cancellationToken);

    public async Task<JobDto> ApproveJobAsync(long jobId, CancellationToken cancellationToken = default)
    {
        await _jobExpiryService.CloseExpiredJobsAsync(cancellationToken);

        var job = await _context.Jobs
            .Include(j => j.Employer)
            .Include(j => j.Images.OrderBy(i => i.Id).Take(1))
            .FirstOrDefaultAsync(j => j.Id == jobId, cancellationToken);

        if (job == null)
        {
            throw new NotFoundException($"Job with id {jobId} was not found.");
        }

        if (!string.Equals(job.PostingStatus, JobPostingCatalog.Pending, StringComparison.OrdinalIgnoreCase))
        {
            throw new BadRequestException("Chỉ có thể duyệt tin đang chờ phê duyệt.");
        }

        job.PostingStatus = JobPostingCatalog.Recruiting;
        await _context.SaveChangesAsync(cancellationToken);

        await _notificationService.NotifyJobApprovedAsync(
            job.Employer.UserId,
            job.Id,
            job.Title,
            cancellationToken);

        return MapJobDto(job);
    }

    public async Task<JobDto> RejectJobAsync(
        long jobId,
        RejectJobRequest? request,
        CancellationToken cancellationToken = default)
    {
        var job = await _context.Jobs
            .Include(j => j.Employer)
            .Include(j => j.Images.OrderBy(i => i.Id).Take(1))
            .FirstOrDefaultAsync(j => j.Id == jobId, cancellationToken);

        if (job == null)
        {
            throw new NotFoundException($"Job with id {jobId} was not found.");
        }

        if (!string.Equals(job.PostingStatus, JobPostingCatalog.Pending, StringComparison.OrdinalIgnoreCase))
        {
            throw new BadRequestException("Chỉ có thể từ chối tin đang chờ phê duyệt.");
        }

        job.PostingStatus = JobPostingCatalog.Rejected;

        var employer = await _context.Employers
            .FirstOrDefaultAsync(e => e.Id == job.EmployerId, cancellationToken);

        if (employer != null)
        {
            employer.PostingLimit++;
            employer.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        await _notificationService.NotifyJobRejectedAsync(
            job.Employer.UserId,
            job.Id,
            job.Title,
            request?.Reason,
            cancellationToken);

        return MapJobDto(job);
    }

    private async Task<PagedResult<AdminPendingJobDto>> GetJobsPagedInternalAsync(
        int page,
        int pageSize,
        string? status,
        string? search,
        CancellationToken cancellationToken)
    {
        if (page < 1)
        {
            page = 1;
        }

        if (pageSize < 1 || pageSize > MaxPageSize)
        {
            pageSize = 12;
        }

        await _jobExpiryService.CloseExpiredJobsAsync(cancellationToken);

        var query = _context.Jobs.AsNoTracking();
        query = ApplyModerationStatusFilter(query, status);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(j =>
                j.Title.Contains(term) ||
                j.Description.Contains(term) ||
                j.Location.Contains(term) ||
                j.Employer.Name.Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(j => j.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(j => new AdminPendingJobDto
            {
                Id = j.Id,
                EmployerId = j.EmployerId,
                EmployerName = j.Employer.Name,
                EmployerEmail = j.Employer.Email,
                EmployerPhone = j.Employer.Phone,
                EmployerImage = j.Employer.Image,
                CategoryId = j.CategoryId,
                CategoryName = j.Category.Name,
                Title = j.Title,
                Description = j.Description,
                DescriptionPreview = j.Description,
                Salary = j.Salary,
                Location = j.Location,
                PostingStatus = j.PostingStatus,
                WorkingHours = j.WorkingHours,
                ApplicantCount = j.Applications.Count,
                ImageCount = j.Images.Count,
                CreatedAt = j.CreatedAt,
                ExpiryDate = j.ExpiryDate,
                ThumbnailUrl = j.Images.OrderBy(i => i.Id).Select(i => i.Url).FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        foreach (var item in items)
        {
            item.DescriptionPreview = JobDescriptionPreview.Create(item.Description);
        }

        return new PagedResult<AdminPendingJobDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    private static IQueryable<Models.Job> ApplyModerationStatusFilter(
        IQueryable<Models.Job> query,
        string? status)
    {
        if (string.IsNullOrWhiteSpace(status) ||
            string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
        {
            return query.Where(j =>
                j.PostingStatus == JobPostingCatalog.Pending ||
                j.PostingStatus == JobPostingCatalog.Recruiting ||
                j.PostingStatus == JobPostingCatalog.Rejected);
        }

        var normalized = status.Trim().ToLowerInvariant();
        return normalized switch
        {
            JobPostingCatalog.Pending => query.Where(j => j.PostingStatus == JobPostingCatalog.Pending),
            JobPostingCatalog.Recruiting => query.Where(j => j.PostingStatus == JobPostingCatalog.Recruiting),
            JobPostingCatalog.Rejected => query.Where(j => j.PostingStatus == JobPostingCatalog.Rejected),
            _ => query.Where(j =>
                j.PostingStatus == JobPostingCatalog.Pending ||
                j.PostingStatus == JobPostingCatalog.Recruiting ||
                j.PostingStatus == JobPostingCatalog.Rejected)
        };
    }

    public async Task<(byte[] Content, string ContentType, string FileName)> ExportJobsByCategoryExcelAsync(
        CancellationToken cancellationToken = default)
    {
        var rows = await _context.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new
            {
                c.Id,
                c.Name,
                JobCount = c.Jobs.Count()
            })
            .ToListAsync(cancellationToken);

        var totalJobs = rows.Sum(r => r.JobCount);
        var generatedAt = DateTime.Now;

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("CongViecTheoDanhMuc");

        worksheet.Cell(1, 1).Value = "BÁO CÁO TỔNG CÔNG VIỆC THEO DANH MỤC";
        worksheet.Range(1, 1, 1, 5).Merge();
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = 15;
        worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        worksheet.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#E8F0FE");

        worksheet.Cell(2, 1).Value = $"Ngày xuất: {generatedAt:dd/MM/yyyy HH:mm:ss}";
        worksheet.Range(2, 1, 2, 5).Merge();
        worksheet.Cell(2, 1).Style.Font.Italic = true;
        worksheet.Cell(2, 1).Style.Font.FontColor = XLColor.FromHtml("#475569");

        var headers = new[] { "STT", "Mã danh mục", "Danh mục", "Số tin tuyển dụng", "Tỷ lệ (%)" };
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
        var stt = 1;
        foreach (var row in rows)
        {
            var percent = totalJobs == 0 ? 0 : Math.Round(row.JobCount * 100.0 / totalJobs, 1);

            worksheet.Cell(rowIndex, 1).Value = stt++;
            worksheet.Cell(rowIndex, 2).Value = row.Id;
            worksheet.Cell(rowIndex, 3).Value = row.Name;
            worksheet.Cell(rowIndex, 4).Value = row.JobCount;
            worksheet.Cell(rowIndex, 5).Value = percent;

            worksheet.Cell(rowIndex, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(rowIndex, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(rowIndex, 5).Style.NumberFormat.Format = "0.0";
            rowIndex++;
        }

        worksheet.Cell(rowIndex + 1, 1).Value = "Tổng cộng";
        worksheet.Range(rowIndex + 1, 1, rowIndex + 1, 3).Merge();
        worksheet.Cell(rowIndex + 1, 1).Style.Font.Bold = true;
        worksheet.Cell(rowIndex + 1, 4).Value = totalJobs;
        worksheet.Cell(rowIndex + 1, 4).Style.Font.Bold = true;
        worksheet.Cell(rowIndex + 1, 5).Value = totalJobs == 0 ? 0 : 100;
        worksheet.Cell(rowIndex + 1, 5).Style.Font.Bold = true;
        worksheet.Cell(rowIndex + 1, 5).Style.NumberFormat.Format = "0.0";

        var tableRange = worksheet.Range(4, 1, Math.Max(4, rowIndex - 1), 5);
        tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        worksheet.Columns(1, 5).AdjustToContents();
        worksheet.SheetView.FreezeRows(4);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var fileName = $"bao-cao-cong-viec-theo-danh-muc-{generatedAt:yyyyMMdd-HHmmss}.xlsx";
        return (stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    public async Task<(byte[] Content, string ContentType, string FileName)> ExportJobsListExcelAsync(
        string? status = null,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        await _jobExpiryService.CloseExpiredJobsAsync(cancellationToken);

        var query = _context.Jobs.AsNoTracking();
        query = ApplyModerationStatusFilter(query, status);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(j =>
                j.Title.Contains(term) ||
                j.Description.Contains(term) ||
                j.Location.Contains(term) ||
                j.Employer.Name.Contains(term));
        }

        var rows = await query
            .OrderByDescending(j => j.Id)
            .Select(j => new
            {
                j.Id,
                j.Title,
                CategoryName = j.Category.Name,
                EmployerName = j.Employer.Name,
                EmployerEmail = j.Employer.Email,
                j.Location,
                j.Salary,
                j.WorkingHours,
                j.PostingStatus,
                j.CreatedAt,
                j.ExpiryDate,
                j.Description
            })
            .ToListAsync(cancellationToken);

        var generatedAt = DateTime.Now;
        var statusLabel = FormatStatusFilterLabel(status);
        var searchLabel = string.IsNullOrWhiteSpace(search) ? "Tất cả" : search.Trim();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("DanhSachCongViec");

        const int columnCount = 13;
        worksheet.Cell(1, 1).Value = "BÁO CÁO DANH SÁCH CÔNG VIỆC";
        worksheet.Range(1, 1, 1, columnCount).Merge();
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = 15;
        worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        worksheet.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#E8F0FE");

        worksheet.Cell(2, 1).Value =
            $"Ngày xuất: {generatedAt:dd/MM/yyyy HH:mm:ss} · Trạng thái: {statusLabel} · Tìm kiếm: {searchLabel}";
        worksheet.Range(2, 1, 2, columnCount).Merge();
        worksheet.Cell(2, 1).Style.Font.Italic = true;
        worksheet.Cell(2, 1).Style.Font.FontColor = XLColor.FromHtml("#475569");

        var headers = new[]
        {
            "STT",
            "Mã tin",
            "Tiêu đề",
            "Danh mục",
            "Nhà tuyển dụng",
            "Email",
            "Địa điểm",
            "Mức lương",
            "Giờ làm việc",
            "Trạng thái",
            "Ngày đăng",
            "Ngày hết hạn",
            "Mô tả"
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
        var stt = 1;
        foreach (var row in rows)
        {
            worksheet.Cell(rowIndex, 1).Value = stt++;
            worksheet.Cell(rowIndex, 2).Value = row.Id;
            worksheet.Cell(rowIndex, 3).Value = row.Title;
            worksheet.Cell(rowIndex, 4).Value = row.CategoryName;
            worksheet.Cell(rowIndex, 5).Value = row.EmployerName;
            worksheet.Cell(rowIndex, 6).Value = row.EmployerEmail ?? string.Empty;
            worksheet.Cell(rowIndex, 7).Value = row.Location;
            worksheet.Cell(rowIndex, 8).Value = row.Salary;
            worksheet.Cell(rowIndex, 9).Value = row.WorkingHours ?? string.Empty;
            worksheet.Cell(rowIndex, 10).Value = FormatPostingStatus(row.PostingStatus);
            worksheet.Cell(rowIndex, 11).Value = row.CreatedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
            worksheet.Cell(rowIndex, 12).Value = row.ExpiryDate.ToLocalTime().ToString("dd/MM/yyyy");
            worksheet.Cell(rowIndex, 13).Value = row.Description;

            worksheet.Cell(rowIndex, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(rowIndex, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(rowIndex, 13).Style.Alignment.WrapText = true;
            rowIndex++;
        }

        worksheet.Cell(rowIndex + 1, 1).Value = "Tổng số tin";
        worksheet.Range(rowIndex + 1, 1, rowIndex + 1, 9).Merge();
        worksheet.Cell(rowIndex + 1, 1).Style.Font.Bold = true;
        worksheet.Cell(rowIndex + 1, 10).Value = rows.Count;
        worksheet.Cell(rowIndex + 1, 10).Style.Font.Bold = true;
        worksheet.Cell(rowIndex + 1, 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        var tableRange = worksheet.Range(4, 1, Math.Max(4, rowIndex - 1), columnCount);
        tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        worksheet.Column(13).Width = 48;
        worksheet.Columns(1, 12).AdjustToContents();
        worksheet.SheetView.FreezeRows(4);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var statusPart = string.IsNullOrWhiteSpace(status) ? "all" : status.Trim().ToLowerInvariant();
        var fileName = $"danh-sach-cong-viec-{statusPart}-{generatedAt:yyyyMMdd-HHmmss}.xlsx";
        return (stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    private static string FormatPostingStatus(string status) => status.Trim().ToLowerInvariant() switch
    {
        JobPostingCatalog.Pending => "Chờ duyệt",
        JobPostingCatalog.Recruiting => "Đang tuyển",
        JobPostingCatalog.Rejected => "Từ chối",
        JobPostingCatalog.Closed => "Đã đóng",
        JobPostingCatalog.Draft => "Nháp",
        _ => status
    };

    private static string FormatStatusFilterLabel(string? status)
    {
        if (string.IsNullOrWhiteSpace(status) ||
            string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
        {
            return "Tất cả (chờ duyệt / đang tuyển / từ chối)";
        }

        return FormatPostingStatus(status);
    }

    private static JobDto MapJobDto(Models.Job job) => new()
    {
        Id = job.Id,
        EmployerId = job.EmployerId,
        EmployerName = job.Employer.Name,
        CategoryId = job.CategoryId,
        Title = job.Title,
        Description = job.Description,
        Salary = job.Salary,
        Location = job.Location,
        PostingStatus = job.PostingStatus,
        WorkingHours = job.WorkingHours,
        ExpiryDate = job.ExpiryDate,
        ThumbnailUrl = job.Images.OrderBy(i => i.Id).Select(i => i.Url).FirstOrDefault()
    };
}
