using JobPortal.Web.Dtos;
using JobPortal.Web.Helpers;
using JobPortal.Web.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JobPortal.Web.Pages.Employer;

public class EditJobModel : PageModel
{
    private const int MaxImageFiles = 8;
    private const long MaxImageBytes = 5 * 1024 * 1024;
    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".webp"
    };

    private readonly ApiService _api;
    private readonly IWebHostEnvironment _env;

    public EditJobModel(ApiService api, IWebHostEnvironment env)
    {
        _api = api;
        _env = env;
    }

    [BindProperty(SupportsGet = true)]
    public long Id { get; set; }

    [BindProperty]
    public JobPostInput Input { get; set; } = new();

    [BindProperty]
    public List<IFormFile> JobImages { get; set; } = new();

    public List<CategoryDto> Categories { get; set; } = new();

    public IReadOnlyList<ImageDto> ExistingImages { get; set; } = Array.Empty<ImageDto>();

    public string PostingStatus { get; set; } = string.Empty;

    public bool CanEdit { get; set; } = true;

    public string? ErrorMessage { get; set; }

    public string? SuccessMessage { get; set; }

    public string ExpiryDateMin => DateTime.UtcNow.ToString("yyyy-MM-dd");

    public string ExpiryDateMax => DateTime.UtcNow.AddMonths(JobExpiryRules.MaxExpiryMonths).ToString("yyyy-MM-dd");

    public async Task<IActionResult> OnGetAsync()
    {
        var redirect = RequireEmployerLogin();
        if (redirect != null)
        {
            return redirect;
        }

        return await LoadPageAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var redirect = RequireEmployerLogin();
        if (redirect != null)
        {
            return redirect;
        }

        if (!await TryLoadJobMetadataAsync(populateInput: false))
        {
            return RedirectToPage("/Employer/Jobs");
        }

        Categories = CategoryDisplayOrder.SortOtherLast(
            await _api.GetApiDataAsync<List<CategoryDto>>("/api/categories") ?? new());

        if (!CanEdit)
        {
            ErrorMessage = "Tin đã đóng hoặc hết hạn, không thể chỉnh sửa.";
            return Page();
        }

        var validation = ValidateInput(Input);
        if (validation != null)
        {
            ErrorMessage = validation;
            return Page();
        }

        var imageInputError = ValidateJobImages(JobImages, ExistingImages.Count);
        if (imageInputError != null)
        {
            ErrorMessage = imageInputError;
            return Page();
        }

        var request = new UpdateEmployerJobRequest
        {
            CategoryId = Input.CategoryId,
            Title = Input.Title.Trim(),
            Description = Input.Description.Trim(),
            Salary = Input.Salary.Trim(),
            Location = Input.Location.Trim(),
            WorkingHours = string.IsNullOrWhiteSpace(Input.WorkingHours) ? null : Input.WorkingHours.Trim(),
            ExpiryDate = JobExpiryRules.NormalizeExpiryDateUtc(Input.ExpiryDate)
        };

        var response = await _api.PutApiResponseAsync<UpdateEmployerJobRequest, EmployerJobEditDto>(
            $"/api/employers/me/jobs/{Id}",
            request);

        if (response is not { Success: true })
        {
            ErrorMessage = response?.Message
                ?? response?.Errors.FirstOrDefault()?.Message
                ?? "Cập nhật tin thất bại.";
            return Page();
        }

        var imageNotes = await SaveJobImagesAndRegisterAsync(Id);
        var wasRejected = string.Equals(PostingStatus, "rejected", StringComparison.OrdinalIgnoreCase);

        TempData["EditJobSuccessMessage"] = string.IsNullOrEmpty(imageNotes)
            ? wasRejected
                ? "Đã lưu tin. Tin chờ admin duyệt lại."
                : "Đã lưu thay đổi."
            : $"Đã lưu thay đổi. {imageNotes}";

        return RedirectToPage(new { id = Id });
    }

    private async Task<IActionResult> LoadPageAsync()
    {
        Categories = CategoryDisplayOrder.SortOtherLast(
            await _api.GetApiDataAsync<List<CategoryDto>>("/api/categories") ?? new());

        if (!await TryLoadJobMetadataAsync(populateInput: true))
        {
            return RedirectToPage("/Employer/Jobs");
        }

        SuccessMessage = TempData["EditJobSuccessMessage"] as string;
        return Page();
    }

    private async Task<bool> TryLoadJobMetadataAsync(bool populateInput)
    {
        var job = await _api.GetApiDataAsync<EmployerJobEditDto>($"/api/employers/me/jobs/{Id}");
        if (job == null)
        {
            return false;
        }

        CanEdit = job.CanEdit;
        PostingStatus = job.PostingStatus;
        ExistingImages = job.Images;

        if (populateInput)
        {
            Input = new JobPostInput
            {
                Title = job.Title,
                Description = job.Description,
                CategoryId = job.CategoryId,
                Salary = job.Salary,
                Location = job.Location,
                WorkingHours = job.WorkingHours,
                ExpiryDate = job.ExpiryDate.Date
            };
        }

        return true;
    }

    private async Task<string> SaveJobImagesAndRegisterAsync(long jobId)
    {
        var nonEmpty = JobImages.Where(f => f.Length > 0).ToList();
        if (nonEmpty.Count == 0)
        {
            return string.Empty;
        }

        var webRoot = _env.WebRootPath;
        if (string.IsNullOrEmpty(webRoot))
        {
            return "Thư mục wwwroot không khả dụng; ảnh chưa được lưu.";
        }

        var physicalDir = Path.Combine(webRoot, "uploads", "job-images", jobId.ToString());
        Directory.CreateDirectory(physicalDir);

        var failures = 0;
        foreach (var file in nonEmpty)
        {
            var ext = Path.GetExtension(file.FileName);
            if (string.IsNullOrEmpty(ext) || !AllowedImageExtensions.Contains(ext))
            {
                failures++;
                continue;
            }

            var storedName = $"{Guid.NewGuid():N}{ext}";
            var physicalPath = Path.Combine(physicalDir, storedName);

            try
            {
                await using (var stream = System.IO.File.Create(physicalPath))
                {
                    await file.CopyToAsync(stream);
                }
            }
            catch
            {
                failures++;
                continue;
            }

            var relativeUrlPath = $"{Request.PathBase}/uploads/job-images/{jobId}/{storedName}".Replace("//", "/");
            if (!relativeUrlPath.StartsWith('/'))
            {
                relativeUrlPath = "/" + relativeUrlPath;
            }

            var publicUrl = $"{Request.Scheme}://{Request.Host}{relativeUrlPath}";
            var displayName = string.IsNullOrWhiteSpace(file.FileName) ? storedName : Path.GetFileName(file.FileName);

            var imgResp = await _api.PostApiResponseAsync<ImageDto, ImageDto>(
                "/api/images",
                new ImageDto
                {
                    Id = 0,
                    JobId = jobId,
                    Url = publicUrl,
                    Name = displayName.Length > 255 ? displayName[..255] : displayName
                });

            if (imgResp is not { Success: true })
            {
                failures++;
                try
                {
                    System.IO.File.Delete(physicalPath);
                }
                catch
                {
                    // ignore cleanup errors
                }
            }
        }

        return failures > 0 ? $"{failures} ảnh không lưu được (kiểm tra định dạng hoặc thử lại)." : string.Empty;
    }

    private IActionResult? RequireEmployerLogin()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken")))
        {
            return RedirectToPage("/Auth/Login", new { returnUrl = HttpContext.Request.Path + HttpContext.Request.QueryString });
        }

        if (!string.Equals(HttpContext.Session.GetString("UserRole"), "EMPLOYER", StringComparison.Ordinal))
        {
            return RedirectToPage("/Index");
        }

        return null;
    }

    private static string? ValidateInput(JobPostInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Title))
        {
            return "Vui lòng nhập tiêu đề tin.";
        }

        if (input.Title.Length > 255)
        {
            return "Tiêu đề không quá 255 ký tự.";
        }

        if (string.IsNullOrWhiteSpace(input.Description))
        {
            return "Vui lòng nhập mô tả công việc.";
        }

        if (input.CategoryId <= 0)
        {
            return "Vui lòng chọn ngành / danh mục.";
        }

        if (string.IsNullOrWhiteSpace(input.Salary))
        {
            return "Vui lòng nhập mức lương.";
        }

        if (input.Salary.Trim().Length > 255)
        {
            return "Mức lương không quá 255 ký tự.";
        }

        if (string.IsNullOrWhiteSpace(input.Location))
        {
            return "Vui lòng nhập địa điểm.";
        }

        if (input.Location.Length > 255)
        {
            return "Địa điểm quá dài.";
        }

        if (input.WorkingHours?.Length > 50)
        {
            return "Thời gian làm việc không quá 50 ký tự.";
        }

        var expiryError = JobExpiryRules.ValidateExpiryDate(input.ExpiryDate);
        if (expiryError != null)
        {
            return expiryError;
        }

        return null;
    }

    private static string? ValidateJobImages(IReadOnlyList<IFormFile> files, int existingImageCount)
    {
        var nonEmpty = files.Where(f => f.Length > 0).ToList();
        if (existingImageCount + nonEmpty.Count > MaxImageFiles)
        {
            return $"Tối đa {MaxImageFiles} ảnh cho mỗi tin (đã có {existingImageCount} ảnh).";
        }

        foreach (var file in nonEmpty)
        {
            if (file.Length > MaxImageBytes)
            {
                return "Mỗi ảnh không vượt quá 5 MB.";
            }

            var ext = Path.GetExtension(file.FileName);
            if (string.IsNullOrEmpty(ext) || !AllowedImageExtensions.Contains(ext))
            {
                return "Chỉ chấp nhận ảnh: jpg, jpeg, png, gif, webp.";
            }
        }

        return null;
    }

    public class JobPostInput
    {
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public long CategoryId { get; set; }

        public string Salary { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public string? WorkingHours { get; set; }

        [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        public DateTime ExpiryDate { get; set; }
    }
}
