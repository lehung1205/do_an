using JobPortal.Web.Dtos;
using JobPortal.Web.Dtos.Auth;
using JobPortal.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JobPortal.Web.Pages.Employer;

public class PostJobModel : PageModel
{
    private readonly ApiService _api;

    public PostJobModel(ApiService api) => _api = api;

    [BindProperty]
    public JobPostInput Input { get; set; } = new();

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
            StartDate = now,
            EndDate = now.AddMonths(3),
            ExpiryDate = now.AddMonths(2)
        };

        var response = await _api.PostApiResponseAsync<JobDto, JobDto>("/api/jobs", dto);
        if (response is not { Success: true })
        {
            ErrorMessage = response?.Message
                ?? response?.Errors.FirstOrDefault()?.Message
                ?? "Đăng tin thất bại.";
            return Page();
        }

        SuccessMessage = "Đã đăng tin tuyển dụng thành công.";
        Input = new JobPostInput();
        return Page();
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

        return null;
    }

    public class JobPostInput
    {
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public long CategoryId { get; set; }

        public int Salary { get; set; }

        public string Location { get; set; } = string.Empty;
    }
}
