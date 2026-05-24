using System.ComponentModel.DataAnnotations;
using JobPortal.Web.Dtos.Auth;
using JobPortal.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JobPortal.Web.Pages.Auth;

public class LoginModel : PageModel
{
    private readonly ApiService _api;

    public LoginModel(ApiService api) => _api = api;

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ErrorMessage { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var response = await _api.PostApiResponseAsync<LoginRequest, AuthResponse>(
            "/api/auth/login",
            new LoginRequest
            {
                Identifier = Input.Identifier.Trim(),
                Password = Input.Password
            });

        if (response is not { Success: true, Data: not null })
        {
            ErrorMessage = response?.Message ?? "Đăng nhập thất bại. Vui lòng thử lại.";
            return Page();
        }

        HttpContext.Session.SetString("JwtToken", response.Data.AccessToken);
        HttpContext.Session.SetString("RefreshToken", response.Data.RefreshToken);
        HttpContext.Session.SetString("UserId", response.Data.User.Id.ToString());
        HttpContext.Session.SetString("UserName", response.Data.User.Name);
        HttpContext.Session.SetString("UserRole", response.Data.User.Role);
        HttpContext.Session.SetString("UserAvatarUrl", response.Data.User.ProfileImage ?? string.Empty);

        if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
        {
            return Redirect(ReturnUrl);
        }

        if (string.Equals(response.Data.User.Role, "ADMIN", StringComparison.Ordinal))
        {
            return RedirectToPage("/Admin/Dashboard/Index");
        }

        return RedirectToPage("/Index");
    }

    public class InputModel
    {
        [Required(ErrorMessage = "Vui lòng nhập email hoặc số điện thoại.")]
        [Display(Name = "Email hoặc số điện thoại")]
        public string Identifier { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = null!;
    }
}
