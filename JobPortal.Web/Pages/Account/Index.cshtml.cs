using System.ComponentModel.DataAnnotations;
using JobPortal.Web.Dtos;
using JobPortal.Web.Dtos.Auth;
using JobPortal.Web.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JobPortal.Web.Pages.Account;

public class IndexModel : PageModel
{
    private const long MaxAvatarBytes = 2 * 1024 * 1024;
    private static readonly HashSet<string> AllowedAvatarExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".webp"
    };

    private readonly ApiService _api;
    private readonly IWebHostEnvironment _env;

    public IndexModel(ApiService api, IWebHostEnvironment env)
    {
        _api = api;
        _env = env;
    }

    public ProfileResponse? Profile { get; set; }

    public EditInputModel EditInput { get; set; } = new();

    public PasswordInputModel PasswordInput { get; set; } = new();

    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }

    public Task<IActionResult> OnGetAsync(string? tab)
    {
        var redirect = RequireLogin();
        if (redirect != null)
        {
            return Task.FromResult(redirect);
        }

        return Task.FromResult(RedirectBack(null, openAccount: true, tab: NormalizeTab(tab ?? "view")));
    }

    public async Task<IActionResult> OnPostUpdateAsync(
        [Bind(Prefix = "EditInput")] EditInputModel? editInput,
        IFormFile? avatarFile,
        [FromForm] string? returnUrl)
    {
        var redirect = RequireLogin();
        if (redirect != null)
        {
            return redirect;
        }

        ModelState.Clear();
        editInput ??= new EditInputModel();

        var current = await _api.GetApiDataAsync<ProfileResponse>("/api/auth/me");
        if (current == null)
        {
            TempData["AccountErrorMessage"] = "Không tải được thông tin tài khoản.";
            TempData["AccountTab"] = "edit";
            return RedirectBack(returnUrl, openAccount: true, tab: "edit");
        }

        var validationError = ValidateEditInput(editInput);
        if (validationError != null)
        {
            TempData["AccountErrorMessage"] = validationError;
            TempData["AccountTab"] = "edit";
            return RedirectBack(returnUrl, openAccount: true, tab: "edit");
        }

        var avatarErr = ValidateAvatarFile(avatarFile);
        if (avatarErr != null)
        {
            TempData["AccountErrorMessage"] = avatarErr;
            TempData["AccountTab"] = "edit";
            return RedirectBack(returnUrl, openAccount: true, tab: "edit");
        }

        string? profileImageUrl = current.ProfileImage;
        if (avatarFile is { Length: > 0 })
        {
            var saved = await TrySaveAvatarAsync(current.Id, avatarFile);
            if (saved.Error != null)
            {
                TempData["AccountErrorMessage"] = saved.Error;
                TempData["AccountTab"] = "edit";
                return RedirectBack(returnUrl, openAccount: true, tab: "edit");
            }

            profileImageUrl = saved.Url;
        }

        var response = await _api.PutApiResponseAsync<UpdateProfileRequest, ProfileResponse>(
            "/api/auth/me",
            BuildUpdateRequest(current, editInput, profileImageUrl));

        if (response is not { Success: true, Data: not null })
        {
            TempData["AccountErrorMessage"] = response?.Message
                ?? response?.Errors.FirstOrDefault()?.Message
                ?? "Cập nhật thông tin thất bại.";
            TempData["AccountTab"] = "edit";
            return RedirectBack(returnUrl, openAccount: true, tab: "edit");
        }

        HttpContext.Session.SetString("UserName", response.Data.Name);
        HttpContext.Session.SetString("UserAvatarUrl", response.Data.ProfileImage ?? string.Empty);
        TempData["AccountSuccessMessage"] = "Đã cập nhật thông tin cá nhân.";
        TempData["AccountTab"] = "view";
        return RedirectBack(returnUrl, openAccount: true, tab: "view");
    }

    public async Task<IActionResult> OnPostChangePasswordAsync(
        [Bind(Prefix = "PasswordInput")] PasswordInputModel? passwordInput,
        [FromForm] string? returnUrl)
    {
        var redirect = RequireLogin();
        if (redirect != null)
        {
            return redirect;
        }

        ModelState.Clear();
        passwordInput ??= new PasswordInputModel();

        var validationError = ValidatePasswordInput(passwordInput);
        if (validationError != null)
        {
            TempData["AccountErrorMessage"] = validationError;
            TempData["AccountTab"] = "password";
            return RedirectBack(returnUrl, openAccount: true, tab: "password");
        }

        var response = await _api.PostApiResponseAsync<ChangePasswordRequest, object>(
            "/api/auth/change-password",
            new ChangePasswordRequest
            {
                CurrentPassword = passwordInput.CurrentPassword,
                NewPassword = passwordInput.NewPassword,
                ConfirmNewPassword = passwordInput.ConfirmNewPassword
            });

        if (response is not { Success: true })
        {
            TempData["AccountErrorMessage"] = response?.Message
                ?? response?.Errors.FirstOrDefault()?.Message
                ?? "Đổi mật khẩu thất bại.";
            TempData["AccountTab"] = "password";
            return RedirectBack(returnUrl, openAccount: true, tab: "password");
        }

        TempData["AccountSuccessMessage"] = "Đã đổi mật khẩu thành công.";
        TempData["AccountTab"] = "password";
        return RedirectBack(returnUrl, openAccount: true, tab: "password");
    }

    public async Task<IActionResult> OnPostAddResumeAsync(
        [Bind(Prefix = "ResumeInput")] ResumeInputModel? resumeInput,
        [FromForm] string? returnUrl)
    {
        var redirect = RequireLogin();
        if (redirect != null)
        {
            return redirect;
        }

        ModelState.Clear();
        resumeInput ??= new ResumeInputModel();

        var validationError = ValidateResumeInput(resumeInput);
        if (validationError != null)
        {
            TempData["AccountErrorMessage"] = validationError;
            TempData["AccountTab"] = "resume";
            TempData["ResumeInputTitle"] = resumeInput.Title;
            TempData["ResumeInputUrl"] = resumeInput.Url;
            return RedirectBack(returnUrl, openAccount: true, tab: "resume");
        }

        var response = await _api.PostApiResponseAsync<CreateResumeRequest, ResumeDto>(
            "/api/resumes/me",
            new CreateResumeRequest
            {
                Title = resumeInput.Title.Trim(),
                Url = resumeInput.Url.Trim()
            });

        if (response is not { Success: true })
        {
            TempData["AccountErrorMessage"] = response?.Message
                ?? response?.Errors.FirstOrDefault()?.Message
                ?? "Thêm hồ sơ thất bại.";
            TempData["AccountTab"] = "resume";
            TempData["ResumeInputTitle"] = resumeInput.Title;
            TempData["ResumeInputUrl"] = resumeInput.Url;
            return RedirectBack(returnUrl, openAccount: true, tab: "resume");
        }

        TempData["AccountSuccessMessage"] = "Đã thêm hồ sơ (CV) thành công.";
        TempData["AccountTab"] = "resume";
        return RedirectBack(returnUrl, openAccount: true, tab: "resume");
    }

    public async Task<IActionResult> OnPostDeleteResumeAsync([FromForm] long resumeId, [FromForm] string? returnUrl)
    {
        var redirect = RequireLogin();
        if (redirect != null)
        {
            return redirect;
        }

        if (resumeId <= 0)
        {
            TempData["AccountErrorMessage"] = "Hồ sơ không hợp lệ.";
            TempData["AccountTab"] = "resume";
            return RedirectBack(returnUrl, openAccount: true, tab: "resume");
        }

        var response = await _api.DeleteApiResponseAsync<object>($"/api/resumes/me/{resumeId}");

        if (response is not { Success: true })
        {
            TempData["AccountErrorMessage"] = response?.Message
                ?? response?.Errors.FirstOrDefault()?.Message
                ?? "Xóa hồ sơ thất bại.";
            TempData["AccountTab"] = "resume";
            return RedirectBack(returnUrl, openAccount: true, tab: "resume");
        }

        TempData["AccountSuccessMessage"] = "Đã xóa hồ sơ. Các đơn ứng tuyển dùng hồ sơ này cũng đã được gỡ bỏ.";
        TempData["AccountTab"] = "resume";
        return RedirectBack(returnUrl, openAccount: true, tab: "resume");
    }

    public static string FormatRole(string role) => role switch
    {
        "JOB_SEEKER" => "Ứng viên",
        "EMPLOYER" => "Nhà tuyển dụng",
        "ADMIN" => "Quản trị viên",
        _ => role
    };

    private static UpdateProfileRequest BuildUpdateRequest(
        ProfileResponse current,
        EditInputModel edit,
        string? profileImageUrl)
    {
        var req = new UpdateProfileRequest
        {
            Name = edit.Name.Trim(),
            PhoneNumber = string.IsNullOrWhiteSpace(edit.PhoneNumber) ? null : edit.PhoneNumber.Trim(),
            ProfileImage = profileImageUrl
        };

        if (string.Equals(current.Role, "JOB_SEEKER", StringComparison.Ordinal))
        {
            req.DateOfBirth = edit.DateOfBirth;
            req.Gender = edit.Gender;
            req.Description = edit.Description;
            req.PermanentAddress = edit.PermanentAddress;
            req.TemporaryAddress = edit.TemporaryAddress;
            req.IdCard = edit.IdCard;
            req.IdCardIssueDate = edit.IdCardIssueDate;
            req.IdCardIssuePlace = edit.IdCardIssuePlace;
            req.BankName = edit.BankName;
            req.BankAccountNumber = edit.BankAccountNumber;
        }
        else if (string.Equals(current.Role, "EMPLOYER", StringComparison.Ordinal))
        {
            req.DateOfBirth = edit.DateOfBirth;
            req.Gender = edit.Gender;
            req.Description = edit.Description;
            req.IdCard = edit.IdCard;
        }
        else if (string.Equals(current.Role, "ADMIN", StringComparison.Ordinal))
        {
            req.BankName = edit.BankName;
            req.BankAccountNumber = edit.BankAccountNumber;
        }

        return req;
    }

    private static string? ValidateEditInput(EditInputModel input)
    {
        if (string.IsNullOrWhiteSpace(input.Name))
        {
            return "Vui lòng nhập họ tên.";
        }

        if (input.Name.Length > 255)
        {
            return "Họ tên không được vượt quá 255 ký tự.";
        }

        if (input.PhoneNumber?.Length > 20)
        {
            return "Số điện thoại không được vượt quá 20 ký tự.";
        }

        if (input.Gender is < 0 or > 2)
        {
            return "Giới tính không hợp lệ.";
        }

        if (input.PermanentAddress?.Length > 500 || input.TemporaryAddress?.Length > 500)
        {
            return "Địa chỉ không được vượt quá 500 ký tự.";
        }

        if (input.IdCard?.Length > 20)
        {
            return "Số CMND/CCCD không được vượt quá 20 ký tự.";
        }

        if (input.IdCardIssueDate?.Length > 50)
        {
            return "Ngày cấp không được vượt quá 50 ký tự.";
        }

        if (input.IdCardIssuePlace?.Length > 255)
        {
            return "Nơi cấp không được vượt quá 255 ký tự.";
        }

        if (input.BankName?.Length > 255)
        {
            return "Tên ngân hàng không được vượt quá 255 ký tự.";
        }

        if (input.BankAccountNumber?.Length > 50)
        {
            return "Số tài khoản không được vượt quá 50 ký tự.";
        }

        return null;
    }

    public static EditInputModel CreateEditInputFromProfile(ProfileResponse profile) => new()
    {
        Name = profile.Name,
        PhoneNumber = profile.PhoneNumber ?? profile.ProfilePhone,
        DateOfBirth = profile.DateOfBirth,
        Gender = profile.Gender,
        Description = profile.Description,
        PermanentAddress = profile.PermanentAddress,
        TemporaryAddress = profile.TemporaryAddress,
        IdCard = profile.IdCard,
        IdCardIssueDate = profile.IdCardIssueDate,
        IdCardIssuePlace = profile.IdCardIssuePlace,
        BankName = profile.BankName,
        BankAccountNumber = profile.BankAccountNumber
    };

    private static string? ValidateAvatarFile(IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            return null;
        }

        if (file.Length > MaxAvatarBytes)
        {
            return "Ảnh đại diện không vượt quá 2 MB.";
        }

        var ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrEmpty(ext) || !AllowedAvatarExtensions.Contains(ext))
        {
            return "Chỉ chấp nhận ảnh: jpg, jpeg, png, gif, webp.";
        }

        return null;
    }

    private async Task<(string? Url, string? Error)> TrySaveAvatarAsync(long userId, IFormFile file)
    {
        var webRoot = _env.WebRootPath;
        if (string.IsNullOrEmpty(webRoot))
        {
            return (null, "Thư mục wwwroot không khả dụng.");
        }

        var ext = Path.GetExtension(file.FileName);
        var storedName = $"{Guid.NewGuid():N}{ext}";
        var dir = Path.Combine(webRoot, "uploads", "avatars", userId.ToString());
        Directory.CreateDirectory(dir);
        var physicalPath = Path.Combine(dir, storedName);

        try
        {
            await using (var stream = System.IO.File.Create(physicalPath))
            {
                await file.CopyToAsync(stream);
            }
        }
        catch
        {
            return (null, "Không lưu được file ảnh.");
        }

        var relativeUrlPath = $"{Request.PathBase}/uploads/avatars/{userId}/{storedName}".Replace("//", "/");
        if (!relativeUrlPath.StartsWith('/'))
        {
            relativeUrlPath = "/" + relativeUrlPath;
        }

        var publicUrl = $"{Request.Scheme}://{Request.Host}{relativeUrlPath}";
        return (publicUrl, null);
    }

    private static string? ValidatePasswordInput(PasswordInputModel input)
    {
        if (string.IsNullOrWhiteSpace(input.CurrentPassword))
        {
            return "Vui lòng nhập mật khẩu hiện tại.";
        }

        if (string.IsNullOrWhiteSpace(input.NewPassword))
        {
            return "Vui lòng nhập mật khẩu mới.";
        }

        if (input.NewPassword.Length < 8)
        {
            return "Mật khẩu mới phải có ít nhất 8 ký tự.";
        }

        if (input.NewPassword != input.ConfirmNewPassword)
        {
            return "Mật khẩu xác nhận không khớp.";
        }

        return null;
    }

    private static string? ValidateResumeInput(ResumeInputModel input)
    {
        if (string.IsNullOrWhiteSpace(input.Title))
        {
            return "Vui lòng nhập tiêu đề hồ sơ.";
        }

        if (input.Title.Length > 255)
        {
            return "Tiêu đề không được vượt quá 255 ký tự.";
        }

        if (string.IsNullOrWhiteSpace(input.Url))
        {
            return "Vui lòng nhập liên kết CV (URL).";
        }

        if (input.Url.Length > 500)
        {
            return "Liên kết không được vượt quá 500 ký tự.";
        }

        if (!Uri.TryCreate(input.Url.Trim(), UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return "Liên kết phải là URL http hoặc https hợp lệ.";
        }

        return null;
    }

    private IActionResult? RequireLogin()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken")))
        {
            return RedirectToPage("/Auth/Login", new { returnUrl = Url.Page("/Index") });
        }

        return null;
    }

    private IActionResult RedirectBack(string? returnUrl, bool openAccount, string tab)
    {
        var target = ResolveReturnUrl(returnUrl);
        if (!openAccount)
        {
            return Redirect(target);
        }

        var separator = target.Contains('?') ? "&" : "?";
        return Redirect($"{target}{separator}accountOpen=1&accountTab={tab}");
    }

    private string ResolveReturnUrl(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return returnUrl;
        }

        return Url.Page("/Index")!;
    }

    private static string NormalizeTab(string tab) => tab switch
    {
        "edit" => "edit",
        "password" => "password",
        "resume" => "resume",
        _ => "view"
    };

    public class EditInputModel
    {
        public string Name { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        public DateOnly? DateOfBirth { get; set; }

        public byte? Gender { get; set; }

        public string? Description { get; set; }

        public string? PermanentAddress { get; set; }

        public string? TemporaryAddress { get; set; }

        public string? IdCard { get; set; }

        public string? IdCardIssueDate { get; set; }

        public string? IdCardIssuePlace { get; set; }

        public string? BankName { get; set; }

        public string? BankAccountNumber { get; set; }
    }

    public class PasswordInputModel
    {
        public string CurrentPassword { get; set; } = string.Empty;

        public string NewPassword { get; set; } = string.Empty;

        public string ConfirmNewPassword { get; set; } = string.Empty;
    }

    public class ResumeInputModel
    {
        public string Title { get; set; } = string.Empty;

        public string Url { get; set; } = string.Empty;
    }
}
