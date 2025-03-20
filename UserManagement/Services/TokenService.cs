using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserManagement.Models;
using UserManagement.Interfaces;
using System.Security.Cryptography;
using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserManagement.DTOs;

namespace UserManagement
{
    public class TokenService : ITokenService
    {
        private readonly UserManager<User> _userManager;
        private readonly JWT _jwt;
        private readonly ILogger<TokenService> _logger;
        public TokenService(UserManager<User> userManager, IOptions<JWT> jwt, ILogger<TokenService> logger)
        {
            _userManager = userManager;
            _jwt = jwt.Value;
            _logger = logger;
        }

        public async Task<JwtSecurityToken> CreateJwtTokenAsync(User user)
        {
            try
            {
                _logger.LogInformation("Creating JWT token for user: {userIdentifier}", user.Id);
                var userClaims = await _userManager.GetClaimsAsync(user);
                var roles = await _userManager.GetRolesAsync(user);
                var roleClaims = roles.Select(role => new Claim("roles", role)).ToList();

                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim("uid", user.Id)
                }
                .Union(userClaims)
                .Union(roleClaims);

                var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
                var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

                _logger.LogInformation("JWT token created successfully for user: {userIdentifier}", user.Id);

                return new JwtSecurityToken(
                    issuer: _jwt.Issuer,
                    audience: _jwt.Audience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(_jwt.DurationInMinutes),
                    signingCredentials: signingCredentials);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating JWT token for user: {userIdentifier}", user.Id);
                throw;
            }
        }

        public RefreshToken GenerateRefreshToken(UserAgent? userAgent)
        {
            try
            {
                _logger.LogInformation("Generating refresh token for user device: {UserDevice}, IP: {UserIp}", userAgent?.UserDevice, userAgent?.UserIp);

                var randomNumber = new byte[32];
                using var generator = new RNGCryptoServiceProvider();
                generator.GetBytes(randomNumber);

                var refreshToken = new RefreshToken
                {
                    Token = Convert.ToBase64String(randomNumber),
                    ExpiresOn = DateTime.UtcNow.AddDays(10),
                    CreatedOn = DateTime.UtcNow,
                    UserDevice = userAgent?.UserDevice,
                    UserIp = userAgent?.UserIp
                };

                _logger.LogInformation("Refresh token generated successfully for device: {UserDevice}, IP: {UserIp}", userAgent?.UserDevice, userAgent?.UserIp);

                return refreshToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating refresh token for device: {UserDevice}, IP: {UserIp}", userAgent?.UserDevice, userAgent?.UserIp);
                throw;
            }
        }

        public async Task<ErrorOr<bool>> RevokeTokenAsync(string token)
        {
            try
            {
                _logger.LogInformation("Attempting to revoke token: {Token}", token);

                var user = await _userManager.Users.Include(e => e.RefreshTokens).SingleOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == token));

                if (user == null)
                {
                    _logger.LogWarning("User not found for token: {Token}", token);
                    return Error.Validation(description: "Invalid token");
                }

                var refreshToken = user.RefreshTokens.SingleOrDefault(t => t.Token == token);

                if (refreshToken == null)
                {
                    _logger.LogWarning("Refresh token not found for token: {Token}", token);
                    return Error.Validation(description: "Refresh token not found");
                }

                if (!refreshToken.IsActive)
                {
                    _logger.LogWarning("Token is already inactive: {Token}", token);
                    return Error.Validation(description: "Inactive token");
                }

                refreshToken.RevokedOn = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                _logger.LogInformation("Token revoked successfully: {Token}", token);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking token: {Token}", token);
                return Error.Failure(description: "Failed to revoke token due to an internal error.");
            }
        }

        public async Task<ErrorOr<AuthModel>> RefreshTokenAsync(RefreshTokenRequest token)
        {
            try
            {
                _logger.LogInformation("Attempting to refresh token: {RefreshToken}", token.RefreshToken);

                var user = await _userManager.Users
                    .Include(r => r.RefreshTokens)
                    .SingleOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == token.RefreshToken));

                if (user == null)
                {
                    _logger.LogWarning("User not found for refresh token: {RefreshToken}", token.RefreshToken);
                    return Error.Validation(description: "Invalid token");
                }

                var refreshToken = user.RefreshTokens.SingleOrDefault(t => t.Token == token.RefreshToken);

                if (refreshToken == null)
                {
                    _logger.LogWarning("Refresh token not found: {RefreshToken}", token.RefreshToken);
                    return Error.Validation(description: "Refresh token not found");
                }

                if (!refreshToken.IsActive)
                {
                    _logger.LogWarning("Refresh token is inactive: {RefreshToken}", token.RefreshToken);
                    return Error.Validation(description: "Inactive token");
                }

                if (refreshToken.UserIp != token.UserIpAddress || refreshToken.UserDevice != token.UserDeviceId)
                {
                    _logger.LogWarning("Unauthorized refresh attempt for token: {RefreshToken} with IP: {UserIp}, Device: {UserDevice}", token.RefreshToken, token.UserIpAddress, token.UserDeviceId);
                    return Error.Unauthorized(description: "Unauthorized, please login again");
                }

                refreshToken.RevokedOn = DateTime.UtcNow;
                var userAgent = new UserAgent { UserDevice = token.UserDeviceId, UserIp = token.UserIpAddress };
                var newRefreshToken = GenerateRefreshToken(userAgent);
                newRefreshToken.UserId = user.Id;

                user.RefreshTokens.Add(newRefreshToken);
                await _userManager.UpdateAsync(user);

                var jwtToken = await CreateJwtTokenAsync(user);

                _logger.LogInformation("Token refreshed successfully for user: {userIdentifier}", user.Id);

                return new AuthModel
                {
                    IsAuthenticated = true,
                    Token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                    Email = user.Email,
                    Username = user.UserName,
                    Roles = (await _userManager.GetRolesAsync(user)).ToList(),
                    RefreshToken = newRefreshToken.Token,
                    RefreshTokenExpiration = newRefreshToken.ExpiresOn
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token: {RefreshToken}", token.RefreshToken);
                return Error.Failure(description: "Failed to refresh token due to an internal error.");
            }
        }
    }
}
