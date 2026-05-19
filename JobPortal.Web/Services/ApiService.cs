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

    public async Task<ApiResponse<T>?> GetApiResponseAsync<T>(string endpoint)
    {
        var res = await CreateClient().GetAsync(endpoint);
        if (!res.IsSuccessStatusCode) return default;
        return await res.Content.ReadFromJsonAsync<ApiResponse<T>>(JsonOptions);
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
        if (res.Content.Headers.ContentLength == 0 && !res.IsSuccessStatusCode)
        {
            return default;
        }

        return await res.Content.ReadFromJsonAsync<ApiResponse<T>>(JsonOptions);
    }

    public async Task<HttpResponseMessage> PutAsync<T>(string endpoint, T data)
    {
        return await CreateClient().PutAsJsonAsync(endpoint, data, JsonOptions);
    }

    public async Task<HttpResponseMessage> DeleteAsync(string endpoint)
    {
        return await CreateClient().DeleteAsync(endpoint);
    }
}