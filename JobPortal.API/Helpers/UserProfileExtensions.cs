using JobPortal.API.Models;
using JobPortal.API.Models.Auth;

namespace JobPortal.API.Helpers;

public static class UserProfileExtensions
{
    public static string GetDisplayName(this User user) =>
        user.JobSeekerProfile?.Name
        ?? user.EmployerProfile?.Name
        ?? user.AdminProfile?.Name
        ?? user.Email;

    public static string? GetProfileImageUrl(this User user) =>
        user.JobSeekerProfile?.ProfileImage
        ?? user.EmployerProfile?.Image
        ?? null;

    public static string GetEmail(this JobSeeker jobSeeker) =>
        jobSeeker.User?.Email ?? string.Empty;

    public static string GetEmail(this Employer employer) =>
        employer.User?.Email ?? string.Empty;

    public static string GetEmail(this Admin admin) =>
        admin.User?.Email ?? string.Empty;

    public static string? GetPhone(this JobSeeker jobSeeker) =>
        jobSeeker.User?.PhoneNumber;

    public static string? GetPhone(this Employer employer) =>
        employer.User?.PhoneNumber;

    public static string? GetPhone(this Admin admin) =>
        admin.User?.PhoneNumber;
}
