using System.Text.Json;

namespace JobPortal.Web.Services;

public static class N8nChatbotResponseParser
{
    public static string? ExtractReply(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            return ExtractFromElement(doc.RootElement);
        }
        catch (JsonException)
        {
            var trimmed = json.Trim();
            if (trimmed.Length > 0 && !trimmed.StartsWith('{') && !trimmed.StartsWith('['))
            {
                return trimmed;
            }

            return null;
        }
    }

    private static string? ExtractFromElement(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.String => element.GetString(),
        JsonValueKind.Array => element.EnumerateArray()
            .Select(ExtractFromElement)
            .FirstOrDefault(s => !string.IsNullOrWhiteSpace(s)),
        JsonValueKind.Object => ExtractFromObject(element),
        _ => null
    };

    private static string? ExtractFromObject(JsonElement obj)
    {
        foreach (var key in new[] { "reply", "message", "output", "text", "response", "answer", "content", "result" })
        {
            if (obj.TryGetProperty(key, out var prop))
            {
                var value = ExtractFromElement(prop);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }
        }

        if (obj.TryGetProperty("data", out var data))
        {
            var nested = ExtractFromElement(data);
            if (!string.IsNullOrWhiteSpace(nested))
            {
                return nested;
            }
        }

        if (obj.TryGetProperty("body", out var body))
        {
            var nested = ExtractFromElement(body);
            if (!string.IsNullOrWhiteSpace(nested))
            {
                return nested;
            }
        }

        return null;
    }
}
