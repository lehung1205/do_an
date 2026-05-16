using System.Diagnostics;

namespace JobPortal.API.DTOs.Common;

public class ApiResponse<T>
{
    public bool Success { get; set; } = true;
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<ApiErrorItem> Errors { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string TraceId { get; set; } = Activity.Current?.Id ?? Guid.NewGuid().ToString();

    public static ApiResponse<T> SuccessResponse(T? data, string message = "")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            Errors = new List<ApiErrorItem>(),
            Timestamp = DateTime.UtcNow
        };
    }

    public static ApiResponse<T> FailResponse(string message, IEnumerable<ApiErrorItem>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Data = default,
            Errors = errors?.ToList() ?? new List<ApiErrorItem>(),
            Timestamp = DateTime.UtcNow
        };
    }
}

public class ApiErrorItem
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
