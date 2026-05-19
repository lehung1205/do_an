namespace JobPortal.Web.Dtos.Common;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<ApiErrorItem> Errors { get; set; } = new();
    public DateTime Timestamp { get; set; }
    public string TraceId { get; set; } = string.Empty;
}

public class ApiErrorItem
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
