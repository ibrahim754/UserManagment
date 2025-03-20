using ErrorOr;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using UserManagement.DTOs;
using UserManagement.Errors;
using UserManagement.Interfaces;
using UserManagement.Models;


public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly ITokenService _tokenService;
    private readonly SignInManager<User> _signInManager;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<User> userManager,
        ITokenService tokenService,
        SignInManager<User> signInManager,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _signInManager = signInManager;
        _logger = logger;
    }

    public async Task<ErrorOr<AuthModel>> LogInAsync(TokenRequestModel model, UserAgent userAgent)
    {
        try
        {
            _logger.LogInformation("Token request received for user: {Email}", model.Email);

            var authModel = new AuthModel();
            var user = await _userManager.FindByEmailAsync(model.Email);
          

            if (user is null)
            {
                _logger.LogWarning("Invalid login attempt for email: {Email}", model.Email);
                return UserErrors.InvalidCredentials ;
            }
            var result = await _signInManager.
                PasswordSignInAsync(user.UserName??" ", model.Password, isPersistent: false, lockoutOnFailure: true);
            if (!result.Succeeded)
            {
                _logger.LogWarning("User {username} failed to logIn", user.UserName);
                return UserErrors.LogInFailed;
            }
            if (result.IsLockedOut)
            {
              
                _logger.LogWarning("User {username} Is Blocked Due To Multiple Login Fails or MissBehave", user.UserName);
                return UserErrors.UserIsLockedOut;
            }
            _logger.LogDebug("Generating JWT for user: {Username}", user.UserName);
            var jwtSecurityToken = await _tokenService.CreateJwtTokenAsync(user);
            var rolesList = await _userManager.GetRolesAsync(user);

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
                _logger.LogInformation("Active refresh token found for user: {Username}", user.UserName);
            }
            else
            {
                var refreshToken = _tokenService.GenerateRefreshToken(userAgent);
                authModel.RefreshToken = refreshToken.Token;
                authModel.RefreshTokenExpiration = refreshToken.ExpiresOn;
                user.RefreshTokens?.Add(refreshToken);
                await _userManager.UpdateAsync(user);
                _logger.LogInformation("New refresh token generated for user: {Username}", user.UserName);
            }

            return authModel;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while generating token.");
            return UserErrors.FetchUsersFailed;
        }
    }

    public Task<ErrorOr<bool>> LogOutAsync(string userIdentifier)
    {
        try
        {

        }
        catch (Exception ex)
        {

        }
        throw new NotImplementedException();
    }
}