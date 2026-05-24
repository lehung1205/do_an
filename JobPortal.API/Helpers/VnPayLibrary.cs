using System.Security.Cryptography;
using System.Text;

namespace JobPortal.API.Helpers;

public class VnPayLibrary
{
    private readonly SortedList<string, string> _requestData = new(new VnPayCompare());
    private readonly SortedList<string, string> _responseData = new(new VnPayCompare());

    public void AddRequestData(string key, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            _requestData[key] = value;
        }
    }

    public void AddResponseData(string key, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            _responseData[key] = value;
        }
    }

    public string GetResponseData(string key)
    {
        return _responseData.TryGetValue(key, out var value) ? value : string.Empty;
    }

    public string CreateRequestUrl(string baseUrl, string hashSecret)
    {
        var queryString = BuildData(_requestData, includeSecureHash: false);
        var secureHash = HmacSha512(hashSecret.Trim(), queryString);
        return $"{baseUrl}?{queryString}&vnp_SecureHash={secureHash}";
    }

    public (string HashData, string SecureHash) CreateDebugSignature(string hashSecret)
    {
        var hashData = BuildData(_requestData, includeSecureHash: false);
        return (hashData, HmacSha512(hashSecret.Trim(), hashData));
    }

    public bool ValidateSignature(string secureHash, string hashSecret)
    {
        var responseData = BuildData(_responseData, includeSecureHash: false);
        var checksum = HmacSha512(hashSecret.Trim(), responseData);
        return string.Equals(checksum, secureHash, StringComparison.OrdinalIgnoreCase);
    }

    private static string BuildData(SortedList<string, string> data, bool includeSecureHash)
    {
        var builder = new StringBuilder();

        foreach (var item in data)
        {
            if (!includeSecureHash &&
                (string.Equals(item.Key, "vnp_SecureHash", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(item.Key, "vnp_SecureHashType", StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(item.Value))
            {
                continue;
            }

            if (builder.Length > 0)
            {
                builder.Append('&');
            }

            builder.Append(item.Key)
                .Append('=')
                .Append(UrlEncode(item.Value));
        }

        return builder.ToString();
    }

    private static string UrlEncode(string value)
    {
        var builder = new StringBuilder();
        foreach (var b in Encoding.UTF8.GetBytes(value))
        {
            if ((b >= 'a' && b <= 'z') ||
                (b >= 'A' && b <= 'Z') ||
                (b >= '0' && b <= '9') ||
                b == '-' ||
                b == '_' ||
                b == '.')
            {
                builder.Append((char)b);
            }
            else if (b == ' ')
            {
                builder.Append('+');
            }
            else
            {
                builder.Append('%').Append(b.ToString("X2"));
            }
        }

        return builder.ToString();
    }

    private static string HmacSha512(string key, string inputData)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var inputBytes = Encoding.UTF8.GetBytes(inputData);

        using var hmac = new HMACSHA512(keyBytes);
        var hashValue = hmac.ComputeHash(inputBytes);
        return Convert.ToHexString(hashValue).ToLowerInvariant();
    }

    private sealed class VnPayCompare : IComparer<string>
    {
        public int Compare(string? x, string? y)
        {
            return string.CompareOrdinal(x, y);
        }
    }
}
