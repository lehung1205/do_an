namespace JobPortal.Web.Services;
using System.Net.Http.Headers;
using System.Threading.Tasks;

public class ApiService
{
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
        return await res.Content.ReadFromJsonAsync<T>();
    }

    public async Task<HttpResponseMessage> PostAsync<T>(string endpoint, T data)
    {
        return await CreateClient().PostAsJsonAsync(endpoint, data);
    }

    public async Task<HttpResponseMessage> PutAsync<T>(string endpoint, T data)
    {
        return await CreateClient().PutAsJsonAsync(endpoint, data);
    }

    public async Task<HttpResponseMessage> DeleteAsync(string endpoint)
    {
        return await CreateClient().DeleteAsync(endpoint);
    }
}