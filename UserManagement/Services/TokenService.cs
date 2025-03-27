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

namespace UserManagement.Services
{
    public class TokenService(UserManager<User> userManager, IOptions<JWT> jwt, ILogger<TokenService> logger)
        : ITokenService
    {
        private readonly JWT _jwt = jwt.Value;

        public async Task<JwtSecurityToken> CreateJwtTokenAsync(User user)
        {

            logger.LogInformation("Creating JWT token for user: {userIdentifier}", user.Id);
            var userClaims = await userManager.GetClaimsAsync(user);
            var roles = await userManager.GetRolesAsync(user);
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

            logger.LogInformation("JWT token created successfully for user: {userIdentifier}", user.Id);

            return new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwt.DurationInMinutes),
                signingCredentials: signingCredentials);

        }

        public RefreshToken GenerateRefreshToken(UserAgent? userAgent)
        {

            logger.LogInformation("Generating refresh token for user device: {UserDevice}, IP: {UserIp}", userAgent?.UserDevice, userAgent?.UserIp);

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

            logger.LogInformation("Refresh token generated successfully for device: {UserDevice}, IP: {UserIp}", userAgent?.UserDevice, userAgent?.UserIp);

            return refreshToken;

        }

        public async Task<ErrorOr<bool>> RevokeTokenAsync(string token)
        {
            logger.LogInformation("Attempting to revoke token: {Token}", token);

            var user = await userManager.Users.Include(e => e.RefreshTokens).SingleOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == token));

            if (user == null)
            {
                logger.LogWarning("User not found for token: {Token}", token);
                return Error.Validation(description: "Invalid token");
            }

            var refreshToken = user.RefreshTokens.SingleOrDefault(t => t.Token == token);

            if (refreshToken == null)
            {
                logger.LogWarning("Refresh token not found for token: {Token}", token);
                return Error.Validation(description: "Refresh token not found");
            }

            if (!refreshToken.IsActive)
            {
                logger.LogWarning("Token is already inactive: {Token}", token);
                return Error.Validation(description: "Inactive token");
            }

            refreshToken.RevokedOn = DateTime.UtcNow;
            await userManager.UpdateAsync(user);

            logger.LogInformation("Token revoked successfully: {Token}", token);

            return true;

        }

        public async Task<ErrorOr<AuthModel>> RefreshTokenAsync(RefreshTokenRequest token)
        {

            logger.LogInformation("Attempting to refresh token: {RefreshToken}", token.RefreshToken);

            var user = await userManager.Users
                .Include(r => r.RefreshTokens)
                .SingleOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == token.RefreshToken));

            if (user == null)
            {
                logger.LogWarning("User not found for refresh token: {RefreshToken}", token.RefreshToken);
                return Error.Validation(description: "Invalid token");
            }

            var refreshToken = user.RefreshTokens.SingleOrDefault(t => t.Token == token.RefreshToken);

            if (refreshToken == null)
            {
                logger.LogWarning("Refresh token not found: {RefreshToken}", token.RefreshToken);
                return Error.Validation(description: "Refresh token not found");
            }

            if (!refreshToken.IsActive)
            {
                logger.LogWarning("Refresh token is inactive: {RefreshToken}", token.RefreshToken);
                return Error.Validation(description: "Inactive token");
            }

            if (refreshToken.UserIp != token.UserIpAddress || refreshToken.UserDevice != token.UserDeviceId)
            {
                logger.LogWarning("Unauthorized refresh attempt for token: {RefreshToken} with IP: {UserIp}, Device: {UserDevice}", token.RefreshToken, token.UserIpAddress, token.UserDeviceId);
                return Error.Unauthorized(description: "Unauthorized, please login again");
            }

            refreshToken.RevokedOn = DateTime.UtcNow;
            var userAgent = new UserAgent { UserDevice = token.UserDeviceId, UserIp = token.UserIpAddress };
            var newRefreshToken = GenerateRefreshToken(userAgent);
            newRefreshToken.UserId = user.Id;

            user.RefreshTokens.Add(newRefreshToken);
            await userManager.UpdateAsync(user);

            var jwtToken = await CreateJwtTokenAsync(user);

            logger.LogInformation("Token refreshed successfully for user: {userIdentifier}", user.Id);

            return new AuthModel
            {
                IsAuthenticated = true,
                Token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                Email = user.Email,
                Username = user.UserName,
                Roles = (await userManager.GetRolesAsync(user)).ToList(),
                RefreshToken = newRefreshToken.Token,
                RefreshTokenExpiration = newRefreshToken.ExpiresOn
            };


        }

    }
}
