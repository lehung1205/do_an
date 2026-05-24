using System.ComponentModel.DataAnnotations;
using JobPortal.Web.Dtos.Auth;
using JobPortal.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JobPortal.Web.Pages.Auth;

public class VerifyRegisterModel : PageModel
{
    private readonly ApiService _api;

    public VerifyRegisterModel(ApiService api) => _api = api;

    [BindProperty(SupportsGet = true)]
    public string? Token { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Email { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public string? SuccessMessage { get; set; }

    public string? DevOtp { get; set; }

    public IActionResult OnGet()
    {
        if (string.IsNullOrWhiteSpace(Token))
        {
            return RedirectToPage("/Auth/Register");
        }

        Input.RegistrationToken = Token;
        DevOtp = TempData["DevOtp"] as string;
        return Page();
    }

    public async Task<IActionResult> OnPostVerifyAsync()
    {
        if (string.IsNullOrWhiteSpace(Input.RegistrationToken))
        {
            return RedirectToPage("/Auth/Register");
        }

        if (!ModelState.IsValid)
        {
            Token = Input.RegistrationToken;
            return Page();
        }

        var response = await _api.PostApiResponseAsync<VerifyRegisterRequest, AuthResponse>(
            "/api/auth/register/verify",
            new VerifyRegisterRequest
            {
                RegistrationToken = Input.RegistrationToken,
                OtpCode = Input.OtpCode.Trim()
            });

        if (response is not { Success: true, Data: not null })
        {
            ErrorMessage = response?.Message ?? "Mã OTP không hợp lệ hoặc đã hết hạn.";
            Token = Input.RegistrationToken;
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

    public async Task<IActionResult> OnPostResendAsync()
    {
        if (string.IsNullOrWhiteSpace(Input.RegistrationToken))
        {
            return RedirectToPage("/Auth/Register");
        }

        var response = await _api.PostApiResponseAsync<ResendRegisterOtpRequest, RegisterPendingResponse>(
            "/api/auth/register/resend-otp",
            new ResendRegisterOtpRequest { RegistrationToken = Input.RegistrationToken });

        Token = Input.RegistrationToken;
        if (response is not { Success: true, Data: not null })
        {
            ErrorMessage = response?.Message ?? "Không thể gửi lại mã OTP.";
            return Page();
        }

        Email = response.Data.Email;
        SuccessMessage = response.Data.Message;
        if (!string.IsNullOrEmpty(response.Data.DevOtp))
        {
            DevOtp = response.Data.DevOtp;
        }

        return Page();
    }

    public class InputModel
    {
        public string RegistrationToken { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập mã OTP.")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã OTP gồm 6 chữ số.")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Mã OTP chỉ gồm 6 chữ số.")]
        [Display(Name = "Mã OTP")]
        public string OtpCode { get; set; } = null!;
    }
}
