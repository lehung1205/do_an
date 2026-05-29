namespace JobPortal.Web.Models;

using System.Text.Json.Serialization;

/// <summary>Payload gửi một lần tới n8n webhook.</summary>
public sealed class N8nChatbotWebhookPayload
{
    public string Message { get; init; } = null!;
    public string Role { get; init; } = null!;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? AccessToken { get; init; }

    public static N8nChatbotWebhookPayload ForAdmin(string message, string role, string? accessToken) =>
        new()
        {
            Message = message,
            Role = role,
            AccessToken = accessToken
        };

    public static N8nChatbotWebhookPayload ForUser(string message, string role) =>
        new()
        {
            Message = message,
            Role = role
        };
}
