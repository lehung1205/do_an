namespace JobPortal.Web.Services;

using System.Net.Http.Headers;
using System.Text.Json;
using JobPortal.Web.Dtos.Common;

public class ApiService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = null
    };

    private readonly IHttpClientFactory _factory;
    private readonly IHttpContextAccessor _accessor;

    public ApiService(IHttpClientFactory factory, IHttpContextAccessor accessor)
    {
        _factory = factory;
        _accessor = accessor;
    }

    private HttpClient CreateClient()
    {
        var client = _factory.CreateClient("JobPortalAPI");

        try
        {
            var token = _accessor.HttpContext?.Session?.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
        }
        catch (InvalidOperationException)
        {
            // Session not available or not configured yet
        }

        return client;
    }

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        var res = await CreateClient().GetAsync(endpoint);
        if (!res.IsSuccessStatusCode) return default;
        return await res.Content.ReadFromJsonAsync<T>(JsonOptions);
    }

    public async Task<HttpResponseMessage> GetRawAsync(string endpoint)
    {
        return await CreateClient().GetAsync(endpoint);
    }

    public async Task<ApiResponse<T>?> GetApiResponseAsync<T>(string endpoint)
    {
        var res = await CreateClient().GetAsync(endpoint);
        return await ReadApiResponseAsync<T>(res);
    }

    public async Task<T?> GetApiDataAsync<T>(string endpoint)
    {
        var response = await GetApiResponseAsync<T>(endpoint);
        return response is { Success: true } ? response.Data : default;
    }

    public async Task<HttpResponseMessage> PostAsync<T>(string endpoint, T data)
    {
        return await CreateClient().PostAsJsonAsync(endpoint, data, JsonOptions);
    }

    public async Task<ApiResponse<T>?> PostApiResponseAsync<TRequest, T>(string endpoint, TRequest data)
    {
        var res = await PostAsync(endpoint, data);
        return await ReadApiResponseAsync<T>(res);
    }

    public async Task<HttpResponseMessage> PutAsync<T>(string endpoint, T data)
    {
        return await CreateClient().PutAsJsonAsync(endpoint, data, JsonOptions);
    }

    public async Task<ApiResponse<T>?> PutApiResponseAsync<TRequest, T>(string endpoint, TRequest data)
    {
        var res = await PutAsync(endpoint, data);
        return await ReadApiResponseAsync<T>(res);
    }

    public async Task<HttpResponseMessage> DeleteAsync(string endpoint)
    {
        return await CreateClient().DeleteAsync(endpoint);
    }

    public async Task<ApiResponse<T>?> DeleteApiResponseAsync<T>(string endpoint)
    {
        var res = await DeleteAsync(endpoint);
        return await ReadApiResponseAsync<T>(res);
    }

    private static async Task<ApiResponse<T>?> ReadApiResponseAsync<T>(HttpResponseMessage res)
    {
        var body = await res.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(body))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<ApiResponse<T>>(body, JsonOptions);
        }
        catch (JsonException)
        {
            return TryParseApiResponseFallback<T>(body);
        }
    }

    private static ApiResponse<T>? TryParseApiResponseFallback<T>(string body)
    {
        try
        {
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            var response = new ApiResponse<T>
            {
                Success = TryGetBool(root, "Success", "success"),
                Message = TryGetString(root, "Message", "message", "title") ?? string.Empty,
                TraceId = TryGetString(root, "TraceId", "traceId") ?? string.Empty
            };

            if (TryGetProperty(root, out var timestampEl, "Timestamp", "timestamp") &&
                timestampEl.ValueKind == JsonValueKind.String &&
                DateTime.TryParse(timestampEl.GetString(), out var timestamp))
            {
                response.Timestamp = timestamp;
            }

            if (TryGetProperty(root, out var dataEl, "Data", "data") &&
                dataEl.ValueKind is not JsonValueKind.Null and not JsonValueKind.Undefined)
            {
                response.Data = dataEl.Deserialize<T>(JsonOptions);
            }

            response.Errors = ParseErrors(root);
            return response;
        }
        catch
        {
            return null;
        }
    }

    private static List<ApiErrorItem> ParseErrors(JsonElement root)
    {
        if (!TryGetProperty(root, out var errorsEl, "Errors", "errors"))
        {
            return new List<ApiErrorItem>();
        }

        if (errorsEl.ValueKind == JsonValueKind.Array)
        {
            return errorsEl.Deserialize<List<ApiErrorItem>>(JsonOptions) ?? new List<ApiErrorItem>();
        }

        if (errorsEl.ValueKind != JsonValueKind.Object)
        {
            return new List<ApiErrorItem>();
        }

        var items = new List<ApiErrorItem>();
        foreach (var property in errorsEl.EnumerateObject())
        {
            if (property.Value.ValueKind == JsonValueKind.Array)
            {
                foreach (var messageEl in property.Value.EnumerateArray())
                {
                    var message = messageEl.GetString();
                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        items.Add(new ApiErrorItem { Field = property.Name, Message = message });
                    }
                }
            }
            else if (property.Value.ValueKind == JsonValueKind.String)
            {
                var message = property.Value.GetString();
                if (!string.IsNullOrWhiteSpace(message))
                {
                    items.Add(new ApiErrorItem { Field = property.Name, Message = message });
                }
            }
        }

        return items;
    }

    private static bool TryGetBool(JsonElement root, params string[] names)
    {
        if (!TryGetProperty(root, out var element, names))
        {
            return false;
        }

        return element.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.String => bool.TryParse(element.GetString(), out var parsed) && parsed,
            _ => false
        };
    }

    private static string? TryGetString(JsonElement root, params string[] names)
    {
        if (!TryGetProperty(root, out var element, names))
        {
            return null;
        }

        return element.ValueKind == JsonValueKind.String ? element.GetString() : null;
    }

    private static bool TryGetProperty(JsonElement root, out JsonElement element, params string[] names)
    {
        foreach (var name in names)
        {
            if (root.TryGetProperty(name, out element))
            {
                return true;
            }
        }

        element = default;
        return false;
    }
}
