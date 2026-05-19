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
    public IndexModel.EditInputModel EditInput { get; set; } = new();
    public IndexModel.PasswordInputModel PasswordInput { get; set; } = new();
    public string UpdateFormAction { get; set; } = "/Account/Index?handler=Update";
    public string ChangePasswordFormAction { get; set; } = "/Account/Index?handler=ChangePassword";
    public string AntiForgeryFieldName { get; set; } = "__RequestVerificationToken";
    public string AntiForgeryRequestToken { get; set; } = string.Empty;

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
}
