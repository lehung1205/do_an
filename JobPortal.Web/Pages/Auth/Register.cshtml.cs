using System.ComponentModel.DataAnnotations;
using JobPortal.Web.Dtos.Auth;
using JobPortal.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JobPortal.Web.Pages.Auth;

public class RegisterModel : PageModel
{
    private readonly ApiService _api;

    public RegisterModel(ApiService api) => _api = api;

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var response = await _api.PostApiResponseAsync<RegisterRequest, AuthResponse>(
            "/api/auth/register",
            new RegisterRequest
            {
                Name = Input.Name.Trim(),
                Email = Input.Email.Trim(),
                PhoneNumber = string.IsNullOrWhiteSpace(Input.PhoneNumber) ? null : Input.PhoneNumber.Trim(),
                Role = Input.Role,
                Password = Input.Password,
                ConfirmPassword = Input.ConfirmPassword
            });

        if (response is not { Success: true, Data: not null })
        {
            ErrorMessage = response?.Message ?? "Đăng ký thất bại. Vui lòng kiểm tra lại thông tin.";
            return Page();
        }

        HttpContext.Session.SetString("JwtToken", response.Data.AccessToken);
        HttpContext.Session.SetString("RefreshToken", response.Data.RefreshToken);
        HttpContext.Session.SetString("UserId", response.Data.User.Id.ToString());
        HttpContext.Session.SetString("UserName", response.Data.User.Name);
        HttpContext.Session.SetString("UserRole", response.Data.User.Role);
        HttpContext.Session.SetString("UserAvatarUrl", response.Data.User.ProfileImage ?? string.Empty);

        return RedirectToPage("/Index");
    }

    public class InputModel : IValidatableObject
    {
        public const string RoleJobSeeker = "JOB_SEEKER";
        public const string RoleEmployer = "EMPLOYER";

        [Required(ErrorMessage = "Vui lòng chọn loại tài khoản.")]
        [Display(Name = "Loại tài khoản")]
        public string Role { get; set; } = RoleJobSeeker;

        [Display(Name = "Họ và tên")]
        public string Name { get; set; } = null!;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                yield return new ValidationResult(
                    Role == RoleEmployer ? "Vui lòng nhập tên công ty." : "Vui lòng nhập họ tên.",
                    [nameof(Name)]);
            }
        }

        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = null!;

        [Display(Name = "Số điện thoại")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu tối thiểu 8 ký tự.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu.")]
        [Compare(nameof(Password), ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        public string ConfirmPassword { get; set; } = null!;
    }
}
