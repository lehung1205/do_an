using System.Globalization;
using JobPortal.Web.Dtos;
using JobPortal.Web.Dtos.Auth;
using JobPortal.Web.Pages.Account;

namespace JobPortal.Web.Models;

public class AccountPanelViewModel
{
    public ProfileResponse Profile { get; set; } = null!;
    public string ReturnUrl { get; set; } = "/";
    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }
    public string ActiveTab { get; set; } = "view";
    public IReadOnlyList<ResumeDto> Resumes { get; set; } = Array.Empty<ResumeDto>();
    public IndexModel.EditInputModel EditInput { get; set; } = new();
    public IndexModel.PasswordInputModel PasswordInput { get; set; } = new();
    public IndexModel.ResumeInputModel ResumeInput { get; set; } = new();
    public string UpdateFormAction { get; set; } = "/Account/Index?handler=Update";
    public string ChangePasswordFormAction { get; set; } = "/Account/Index?handler=ChangePassword";
    public string AddResumeFormAction { get; set; } = "/Account/Index?handler=AddResume";
    public string DeleteResumeFormAction { get; set; } = "/Account/Index?handler=DeleteResume";
    public string AntiForgeryFieldName { get; set; } = "__RequestVerificationToken";
    public string AntiForgeryRequestToken { get; set; } = string.Empty;

    public bool IsJobSeeker => string.Equals(Profile.Role, "JOB_SEEKER", StringComparison.Ordinal);

    public bool IsAdmin => string.Equals(Profile.Role, "ADMIN", StringComparison.Ordinal);

    public bool IsEmployer => string.Equals(Profile.Role, "EMPLOYER", StringComparison.Ordinal);

    /// <summary>Giá trị cho input type="date" (yyyy-MM-dd).</summary>
    public static string? FormatDateOnlyInput(DateOnly? value) =>
        value?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    public static string FormatRole(string role) => IndexModel.FormatRole(role);

    public static string GetInitials(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "?";
        }

        var parts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1)
        {
            return parts[0][..1].ToUpperInvariant();
        }

        return $"{parts[0][0]}{parts[^1][0]}".ToUpperInvariant();
    }

    private static readonly CultureInfo Vi = CultureInfo.GetCultureInfo("vi-VN");

    public static string DashIfEmpty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "—" : value;

    public static string FormatDateOnly(DateOnly? value) =>
        value == null ? "—" : value.Value.ToString("dd/MM/yyyy", Vi);

    public static string FormatDateTime(DateTime value) =>
        value.ToLocalTime().ToString("dd/MM/yyyy HH:mm", Vi);

    public static string FormatDateTimeNullable(DateTime? value) =>
        value == null ? "—" : value.Value.ToLocalTime().ToString("dd/MM/yyyy HH:mm", Vi);

    /// <summary>Giả định: 0 = khác, 1 = nam, 2 = nữ (điều chỉnh nếu seed DB khác).</summary>
    public static string FormatGender(byte? gender) => gender switch
    {
        1 => "Nam",
        2 => "Nữ",
        0 => "Khác",
        _ => "—"
    };

    public static string FormatAccountStatus(string? status) =>
        string.IsNullOrWhiteSpace(status) ? "—" : status.Trim().ToUpperInvariant() switch
        {
            "ACTIVE" => "Đang hoạt động",
            "INACTIVE" => "Không hoạt động",
            "SUSPENDED" => "Tạm khóa",
            _ => status
        };
}
