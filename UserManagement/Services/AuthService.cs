using ErrorOr;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using UserManagement.DTOs;
using UserManagement.Errors;
using UserManagement.Interfaces;
using UserManagement.Models;

namespace UserManagement.Services
{
    public class AuthService(
        UserManager<User> userManager,
        ITokenService tokenService,
        SignInManager<User> signInManager,
        ILogger<AuthService> logger)
        : IAuthService
    {
        public async Task<ErrorOr<AuthModel>> LogInAsync(TokenRequestModel model, UserAgent? userAgent)
        {

            logger.LogInformation("Token request received for user: {Email}", model.Email);

            var authModel = new AuthModel();
            var user = await userManager.FindByEmailAsync(model.Email);


            if (user is null)
            {
                logger.LogWarning("Invalid login attempt for email: {Email}", model.Email);
                return UserErrors.InvalidCredentials;
            }
            var result = await signInManager.
                PasswordSignInAsync(user.UserName ?? " ", model.Password, isPersistent: false, lockoutOnFailure: true);
            if (!result.Succeeded)
            {
                logger.LogWarning("User {username} failed to logIn", user.UserName);
                return UserErrors.LogInFailed;
            }
            if (result.IsLockedOut)
            {

                logger.LogWarning("User {username} Is Blocked Due To Multiple Login Fails or MissBehave", user.UserName);
                return UserErrors.UserIsLockedOut;
            }
            logger.LogDebug("Generating JWT for user: {UserName}", user.UserName);
            var jwtSecurityToken = await tokenService.CreateJwtTokenAsync(user);
            var rolesList = await userManager.GetRolesAsync(user);

            authModel.IsAuthenticated = true;
            authModel.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            authModel.Email = user.Email;
            authModel.Username = user.UserName;
            authModel.ExpiresOn = jwtSecurityToken.ValidTo;
            authModel.Roles = rolesList.ToList();

            if (user.RefreshTokens != null && user.RefreshTokens.Any(t => t.IsActive))
            {
                var activeRefreshToken = user.RefreshTokens.FirstOrDefault(t => t.IsActive);
                authModel.RefreshToken = activeRefreshToken?.Token;
                authModel.RefreshTokenExpiration = activeRefreshToken.ExpiresOn;
                logger.LogInformation("Active refresh token found for user: {UserName}", user.UserName);
            }
            else
            {
                var refreshToken = tokenService.GenerateRefreshToken(userAgent);
                authModel.RefreshToken = refreshToken.Token;
                authModel.RefreshTokenExpiration = refreshToken.ExpiresOn;
                user.RefreshTokens?.Add(refreshToken);
                await userManager.UpdateAsync(user);
                logger.LogInformation("New refresh token generated for user: {UserName}", user.UserName);
            }

            return authModel;

        }

        public Task<ErrorOr<bool>> LogOutAsync(string userIdentifier)
        {

            throw new NotImplementedException();
        }
    }
}