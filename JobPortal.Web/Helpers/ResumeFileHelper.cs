namespace JobPortal.Web.Helpers;

public static class ResumeFileHelper
{
    public const long MaxResumePdfBytes = 10 * 1024 * 1024;

    private static readonly HashSet<string> AllowedResumeExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf"
    };

    public static string? ValidateTitle(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return "Vui lòng nhập tiêu đề hồ sơ.";
        }

        if (title.Length > 255)
        {
            return "Tiêu đề không được vượt quá 255 ký tự.";
        }

        return null;
    }

    public static string? ValidatePdfFile(IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            return "Vui lòng chọn file CV (PDF).";
        }

        if (file.Length > MaxResumePdfBytes)
        {
            return "File CV không được vượt quá 10 MB.";
        }

        var ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrEmpty(ext) || !AllowedResumeExtensions.Contains(ext))
        {
            return "Chỉ chấp nhận file PDF (.pdf).";
        }

        var contentType = file.ContentType?.Trim() ?? string.Empty;
        if (!string.IsNullOrEmpty(contentType) &&
            !contentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase) &&
            !contentType.Equals("application/octet-stream", StringComparison.OrdinalIgnoreCase))
        {
            return "File phải có định dạng PDF.";
        }

        return null;
    }

    public static async Task<(string? Url, string? Error)> TrySaveResumePdfAsync(
        HttpRequest request,
        IWebHostEnvironment env,
        long userId,
        IFormFile file)
    {
        var webRoot = env.WebRootPath;
        if (string.IsNullOrEmpty(webRoot))
        {
            return (null, "Thư mục wwwroot không khả dụng.");
        }

        var ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrEmpty(ext) || !AllowedResumeExtensions.Contains(ext))
        {
            return (null, "Chỉ chấp nhận file PDF (.pdf).");
        }

        var storedName = $"{Guid.NewGuid():N}{ext}";
        var dir = Path.Combine(webRoot, "uploads", "resumes", userId.ToString());
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
            return (null, "Không lưu được file CV.");
        }

        var relativeUrlPath = $"{request.PathBase}/uploads/resumes/{userId}/{storedName}".Replace("//", "/");
        if (!relativeUrlPath.StartsWith('/'))
        {
            relativeUrlPath = "/" + relativeUrlPath;
        }

        var publicUrl = $"{request.Scheme}://{request.Host}{relativeUrlPath}";
        if (publicUrl.Length > 500)
        {
            try
            {
                System.IO.File.Delete(physicalPath);
            }
            catch
            {
                // ignore cleanup errors
            }

            return (null, "URL file CV quá dài. Vui lòng liên hệ quản trị.");
        }

        return (publicUrl, null);
    }

    public static void TryDeleteResumeFile(IWebHostEnvironment env, string? resumeUrl)
    {
        if (string.IsNullOrWhiteSpace(resumeUrl))
        {
            return;
        }

        var webRoot = env.WebRootPath;
        if (string.IsNullOrEmpty(webRoot))
        {
            return;
        }

        if (!Uri.TryCreate(resumeUrl.Trim(), UriKind.Absolute, out var uri))
        {
            return;
        }

        var path = uri.AbsolutePath;
        const string prefix = "/uploads/resumes/";
        var idx = path.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
        if (idx < 0)
        {
            return;
        }

        var relative = path[idx..].TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var physicalPath = Path.Combine(webRoot, relative);
        var fullRoot = Path.GetFullPath(webRoot);
        var fullFile = Path.GetFullPath(physicalPath);
        if (!fullFile.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        try
        {
            if (System.IO.File.Exists(fullFile))
            {
                System.IO.File.Delete(fullFile);
            }
        }
        catch
        {
            // ignore cleanup errors
        }
    }
}
