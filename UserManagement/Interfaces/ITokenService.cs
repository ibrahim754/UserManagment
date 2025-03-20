using ErrorOr;
using System.IdentityModel.Tokens.Jwt;
using UserManagement.DTOs;
using UserManagement.Models;

namespace UserManagement.Interfaces
{
    public interface ITokenService
    {
        Task<JwtSecurityToken> CreateJwtTokenAsync(User user);
        RefreshToken GenerateRefreshToken(UserAgent? userAgent);
        Task<ErrorOr<bool>> RevokeTokenAsync(string token);
        Task<ErrorOr<AuthModel>> RefreshTokenAsync(RefreshTokenRequest token);

    }
}
