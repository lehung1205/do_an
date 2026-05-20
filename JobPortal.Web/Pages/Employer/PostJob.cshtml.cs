using JobPortal.Web.Dtos;
using JobPortal.Web.Dtos.Auth;
using JobPortal.Web.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JobPortal.Web.Pages.Employer;

public class PostJobModel : PageModel
{
    private const int MaxImageFiles = 8;
    private const long MaxImageBytes = 5 * 1024 * 1024;
    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".webp"
    };

    private readonly ApiService _api;
    private readonly IWebHostEnvironment _env;

    public PostJobModel(ApiService api, IWebHostEnvironment env)
    {
        _api = api;
        _env = env;
    }

    [BindProperty]
    public JobPostInput Input { get; set; } = new();

    [BindProperty]
    public List<IFormFile> JobImages { get; set; } = new();

    public List<CategoryDto> Categories { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var redirect = RequireEmployerLogin();
        if (redirect != null)
        {
            return redirect;
        }

        Categories = await _api.GetApiDataAsync<List<CategoryDto>>("/api/categories") ?? new();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var redirect = RequireEmployerLogin();
        if (redirect != null)
        {
            return redirect;
        }

        Categories = await _api.GetApiDataAsync<List<CategoryDto>>("/api/categories") ?? new();

        var profile = await _api.GetApiDataAsync<ProfileResponse>("/api/auth/me");
        if (profile?.EmployerId is not long employerId)
        {
            ErrorMessage = "Không tìm thấy thông tin nhà tuyển dụng. Vui lòng đăng nhập lại.";
            return Page();
        }

        var validation = ValidateInput(Input);
        if (validation != null)
        {
            ErrorMessage = validation;
            return Page();
        }

        var imageInputError = ValidateJobImages(JobImages);
        if (imageInputError != null)
        {
            ErrorMessage = imageInputError;
            return Page();
        }

        var now = DateTime.UtcNow;
        var dto = new JobDto
        {
            Id = 0,
            EmployerId = employerId,
            CategoryId = Input.CategoryId,
            Title = Input.Title.Trim(),
            Description = Input.Description.Trim(),
            Salary = Input.Salary,
            Location = Input.Location.Trim(),
            PostingStatus = "recruiting",
            WorkingHours = string.IsNullOrWhiteSpace(Input.WorkingHours) ? null : Input.WorkingHours.Trim(),
            ExpiryDate = now.AddMonths(2)
        };

        var response = await _api.PostApiResponseAsync<JobDto, JobDto>("/api/jobs", dto);
        if (response is not { Success: true, Data: not null })
        {
            ErrorMessage = response?.Message
                ?? response?.Errors.FirstOrDefault()?.Message
                ?? "Đăng tin thất bại.";
            return Page();
        }

        var newJobId = response.Data.Id;
        var imageNotes = await SaveJobImagesAndRegisterAsync(newJobId);

        SuccessMessage = string.IsNullOrEmpty(imageNotes)
            ? "Đã đăng tin tuyển dụng thành công."
            : $"Đã đăng tin tuyển dụng thành công. {imageNotes}";

        Input = new JobPostInput();
        JobImages = new List<IFormFile>();
        return Page();
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

        if (input.Salary < 0)
        {
            return "Mức lương không hợp lệ.";
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

        return null;
    }

    private static string? ValidateJobImages(IReadOnlyList<IFormFile> files)
    {
        var nonEmpty = files.Where(f => f.Length > 0).ToList();
        if (nonEmpty.Count > MaxImageFiles)
        {
            return $"Tối đa {MaxImageFiles} ảnh đính kèm.";
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

        public int Salary { get; set; }

        public string Location { get; set; } = string.Empty;

        public string? WorkingHours { get; set; }
    }
}
