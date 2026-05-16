using JobPortal.API.Models.Auth;

namespace JobPortal.API.Services.Auth;

public interface ITokenService
{
    string CreateAccessToken(User user);
    (RefreshToken token, string value) GenerateRefreshToken(string ipAddress);
    string HashToken(string token);
    bool VerifyToken(string token, string hash);
}