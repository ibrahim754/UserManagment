﻿using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using UserManagement.Models;
using ErrorOr;
using UserManagement.DAL;
using Microsoft.Extensions.Logging;
using UserManagement.Interfaces;
using UserManagement.DTOs;
using UserManagement.Errors;
using UserManagement.Entites;
using System.Security.Cryptography;

namespace UserManagement
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly JWT _jwt;
        private readonly ILogger<UserService> _logger;
        private readonly ITokenService _tokenService;
        private readonly UserManagmentDbContext _UserManagmentDbContext;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly SignInManager<User> _signInManager;
        private readonly IMailService _mailService;
        private readonly ICacheService _cacheService;   
        public UserService(UserManager<User> userManager, RoleManager<IdentityRole> roleManager,
            IOptions<JWT> jwt, ITokenService tokenService, ILogger<UserService> logger, UserManagmentDbContext userManagmentDbContext,
            ICloudinaryService cloudinaryService, SignInManager<User> signInManager, IMailService mailService,ICacheService cacheService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwt = jwt.Value;
            _tokenService = tokenService;
            _logger = logger;
            _UserManagmentDbContext = userManagmentDbContext;
            _cloudinaryService = cloudinaryService;
            _signInManager = signInManager;
            _mailService = mailService;
            _cacheService = cacheService;

        }

        public async Task<ErrorOr<Guid>> RegisterAsync(RegisterModel model, UserAgent userAgent)
        {
            try
            {
                _logger.LogInformation("Starting registration process for user with email: {Email}", model.Email);

                if (await _userManager.FindByEmailAsync(model.Email) is not null)
                {
                    _logger.LogWarning("Registration attempt with existing email: {Email}", model.Email);
                    return UserErrors.EmailAlreadyRegistered;
                }

                if (await _userManager.FindByNameAsync(model.Username) is not null)
                {
                    _logger.LogWarning("Registration attempt with existing username: {Username}", model.Username);
                    return UserErrors.UsernameAlreadyRegistered;
                }

                _logger.LogInformation("User device ID: {Device}, IP: {Ip} trying to register new account with email {email}", userAgent.UserDevice, userAgent.UserIp, model.Email);
                PendingRegistration confirmationUserModel = new PendingRegistration()
                {
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Password = model.Password,
                    Username = model.Username
                };
                if (model.Image is not null)
                {
                    _logger.LogInformation($"uploading image for the user {model.Username}");
                    var cloudResult = await _cloudinaryService.UploadImageAsync(model.Image);
                    if (cloudResult.IsError)
                    {
                        _logger.LogWarning("Error Occoured while Saving the user image");
                        return cloudResult.Errors.FirstOrDefault();
                    }
                    confirmationUserModel.Image = cloudResult.Value;
                }
                int confirmationCodeDuration = 15;
                var registerProccessId = Guid.NewGuid();    
                confirmationUserModel.ConfirmationCode = GenerateSecureConfirmationCode();
                var sendEmailResult =  await _mailService.SendEmailAsync(model.Email,"Email Confirmation",
                   $"Dear User {model.FirstName} {model.LastName}\n," +
                   $"Please confirm your registration\n" +
                   $"Here is your confirmation code : {confirmationUserModel.ConfirmationCode}\n" +
                   $"Please Note that the confirmation code is only valid for {confirmationCodeDuration} mintues\n" +
                   $"With All Wishes");
                if(sendEmailResult.IsError)
                {
                    return sendEmailResult.Errors.FirstOrDefault(); 
                }
                _cacheService.AddToCache(new CacheItem { Key = registerProccessId.ToString(), Value = confirmationUserModel }, confirmationCodeDuration * 60);
                return  registerProccessId;
                 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during registration.");
                return UserErrors.FetchUsersFailed;
            }
        }
        public async Task<ErrorOr<AuthModel>> ConfirmRegisterAsync(ConfirmationUserDto confirmationUser, UserAgent userAgent)
        {
            try
            {
                 var cacheResult = _cacheService.GetCacheItemByKey(confirmationUser.registerationId.ToString());
                if (cacheResult.IsError)
                {
                    return cacheResult.Errors.FirstOrDefault();
                }
                if (!(cacheResult.Value is PendingRegistration model))
                {
                    return Error.Failure("Cache item type conversion to ConfirmationUserModel failed.");
                }
                if(model.ConfirmationCode != confirmationUser.confirmationCode)
                {
                    return Error.Failure("Invalid confirmation code.");
                }
                var user = new User
                {
                    UserName = model.Username,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Image = model.Image
                };
                var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var result = await _userManager.CreateAsync(user, model.Password);

             

                await _userManager.AddToRoleAsync(user, "User");
                _logger.LogInformation("User {Username} assigned role: User", model.Username);

                var jwtSecurityToken = await _tokenService.CreateJwtTokenAsync(user);
                var refreshToken = _tokenService.GenerateRefreshToken(userAgent);
                refreshToken.UserId = user.Id;

                user.RefreshTokens?.Add(refreshToken);
                await _userManager.UpdateAsync(user);
                _logger.LogInformation("User {Username} registered successfully with refresh token.", model.Username);

                return new AuthModel
                {
                    Email = user.Email,
                    ExpiresOn = jwtSecurityToken.ValidTo,
                    IsAuthenticated = true,
                    Roles = new List<string> { "User" },
                    Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                    Username = user.UserName,
                    RefreshToken = refreshToken.Token,
                    RefreshTokenExpiration = refreshToken.ExpiresOn
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during registration.");
                return UserErrors.FetchUsersFailed;
            }
        }
        public async Task<ErrorOr<AuthModel>> LogInAsync(TokenRequestModel model, UserAgent userAgent)
        {
            try
            {
                _logger.LogInformation("Token request received for user: {Email}", model.Email);

                var authModel = new AuthModel();
                var user = await _userManager.Users.Include(e => e.RefreshTokens).FirstOrDefaultAsync(e => e.Email == model.Email);

                if (user is null )
                {
                    _logger.LogWarning("Invalid login attempt for email: {Email}", model.Email);
                    return UserErrors.IncorrectPasswordOrEmail;
                }
                var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, isPersistent: false, lockoutOnFailure: true);
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User {username} Is Blocked Due To Multiple Login Fails", user.UserName);
                    return UserErrors.UserIsLockedOut;
                }
                else if (!result.Succeeded)
                {
                    _logger.LogWarning("User {username} failed to logIn", user.UserName);
                    return UserErrors.LogInFailed;
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

                if (user.RefreshTokens.Any(t => t.IsActive))
                {
                    var activeRefreshToken = user.RefreshTokens.FirstOrDefault(t => t.IsActive);
                    authModel.RefreshToken = activeRefreshToken.Token;
                    authModel.RefreshTokenExpiration = activeRefreshToken.ExpiresOn;
                    _logger.LogInformation("Active refresh token found for user: {Username}", user.UserName);
                }
                else
                {
                    var refreshToken = _tokenService.GenerateRefreshToken(userAgent);
                    authModel.RefreshToken = refreshToken.Token;
                    authModel.RefreshTokenExpiration = refreshToken.ExpiresOn;
                    user.RefreshTokens.Add(refreshToken);
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

        public async Task<ErrorOr<string>> ChangePasswordAsync(ChangePasswordRequest changePassword)
        {
            try
            {
                _logger.LogInformation("Change password request for user: {UserId}", changePassword.UserId);

                var user = await _userManager.FindByIdAsync(changePassword.UserId);
                if (user is null)
                {
                    _logger.LogWarning("User not found for password change: {UserId}", changePassword.UserId);
                    return UserErrors.UserNotFound;
                }

                var checkPassword = await _userManager.CheckPasswordAsync(user, changePassword.CurrentPassword);
                if (!checkPassword)
                {
                    _logger.LogWarning("Password change attempt with incorrect current password for user: {UserId}", changePassword.UserId);
                    return UserErrors.IncorrectPassword;
                }

                var result = await _userManager.ChangePasswordAsync(user, changePassword.CurrentPassword, changePassword.NewPassword);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError("Failed to change password for user: {UserId}. Errors: {Errors}", changePassword.UserId, errors);
                    return UserErrors.ChangePasswordFailed;
                }

                _logger.LogInformation("Password changed successfully for user: {UserId}", changePassword.UserId);
                return "Changed Successfully";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while changing password.");
                return UserErrors.FetchUsersFailed;
            }
        }

        public async Task<ErrorOr<IReadOnlyCollection<User>>> BrowseAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all users");

                var users = await _userManager.Users.Include(e => e.RefreshTokens).AsNoTracking().ToListAsync();
             
                _logger.LogInformation("Retrieved {UserCount} users from the database", users.Count);
                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving users.");
                return UserErrors.FetchUsersFailed;
            }
        }

        public async Task<ErrorOr<bool>> BlockUser(string userId)
        {
            try
            {
                 
                var user = await _userManager.FindByIdAsync(userId);
                if (user is null)
                {
                    _logger.LogWarning("User is Not Found");
                    return UserErrors.UserNotFound;
                }
                user.LockoutEnabled = true;

                var result =await  _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return true;
                }            
                return UserErrors.FetchUsersFailed;
            }
            catch (Exception ex1)
            {
                return UserErrors.FetchUsersFailed;

            }
        }
        private static string GenerateSecureConfirmationCode()
        {
             
            const int minValue = 100000;
            const int maxValue = 999999 + 1;  

           
            byte[] randomBytes = new byte[4];  
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            int randomNumber = BitConverter.ToInt32(randomBytes, 0);

            randomNumber = Math.Abs(randomNumber % (maxValue - minValue)) + minValue;

            return randomNumber.ToString("D6");  
        }


    }
}
