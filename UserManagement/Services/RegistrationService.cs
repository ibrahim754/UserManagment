using ErrorOr;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using UserManagement.Constans;
using UserManagement.DTOs;
using UserManagement.Entites;
using UserManagement.Errors;
using UserManagement.Interfaces;
using UserManagement.Models;

namespace UserManagement.Services
{
    public class RegistrationService : IRegistrationService
    {
        private readonly UserManager<User> _userManager;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IMailService _mailService;
        private readonly ICacheService _cacheService;
        private readonly ITokenService _tokenService;
        private readonly ILogger<RegistrationService> _logger;

        public RegistrationService(
            UserManager<User> userManager,
            ICloudinaryService cloudinaryService,
            IMailService mailService,
            ICacheService cacheService,
            ITokenService tokenService,
            ILogger<RegistrationService> logger)
        {
            _userManager = userManager;
            _cloudinaryService = cloudinaryService;
            _mailService = mailService;
            _cacheService = cacheService;
            _tokenService = tokenService;
            _logger = logger;
        }

        public async Task<ErrorOr<Guid>> RegisterAsync(RegisterModel model, UserAgent? userAgent, List<string>? roles)
        {

            _logger.LogInformation("Starting registration process for user with email: {Email}", model.Email);

            roles ??= [DefaultRoles.User.ToString()];

            if (await _userManager.FindByEmailAsync(model.Email) is not null)
            {
                _logger.LogWarning("Registration attempt with existing email: {Email}", model.Email);
                return UserErrors.EmailAlreadyRegistered;
            }

            if (await _userManager.FindByNameAsync(model.UserName) is not null)
            {
                _logger.LogWarning("Registration attempt with existing username: {UserName}", model.UserName);
                return UserErrors.UsernameAlreadyRegistered;
            }
            _logger.LogInformation("User device ID: {Device}, IP: {Ip} trying to register new account with email {email}", userAgent?.UserDevice, userAgent?.UserIp, model.Email);
            PendingRegistration confirmationUserModel = new PendingRegistration()
            {
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Password = model.Password,
                Username = model.UserName,
                roles = roles
            };
            if (model.Image is not null)
            {
                _logger.LogInformation($"uploading image for the user {model.UserName}");
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
            MailRequestDto mail = new MailRequestDto()
            {
                mailTo = model.Email,
                Subject = "Email Confirmation",
                Body = $"Dear User {model.FirstName} {model.LastName}\n," +
                       $"Please confirm your registration\n" +
                       $"Here is your confirmation code : {confirmationUserModel.ConfirmationCode}\n" +
                       $"Please Note that the confirmation code is only valid for {confirmationCodeDuration} mintues\n" +
                       $"With All Wishes"
            };
            var sendEmailResult = await _mailService.SendEmailAsync(mail);
            if (sendEmailResult.IsError)
            {
                return sendEmailResult.Errors.FirstOrDefault();
            }
            _cacheService.AddToCache(new CacheItem { Key = registerProccessId.ToString(), Value = confirmationUserModel }, confirmationCodeDuration * 60);
            return registerProccessId;


        }

        public async Task<ErrorOr<AuthModel>> ConfirmRegisterAsync(ConfirmationUserDto confirmationUser, UserAgent? userAgent)
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
            if (model.ConfirmationCode != confirmationUser.confirmationCode)
            {
                return Error.Failure("Invalid confirmation code.");
            }
            var user = new User
            {
                UserName = model.Username,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Image = model.Image,
                EmailConfirmed = true
            };
            return await CreateUserAsync(user, model.Password, model.roles ?? [], userAgent);

        }

        public async Task<ErrorOr<AuthModel>> CreateUserAsync(User user, string password, List<string> roles, UserAgent? userAgent)
        {

            var result = await _userManager.CreateAsync(user, password);
            await _userManager.AddToRolesAsync(user, roles);
            _logger.LogInformation("User {UserName} assigned role: User", user.UserName);

            var jwtSecurityToken = await _tokenService.CreateJwtTokenAsync(user);
            var refreshToken = _tokenService.GenerateRefreshToken(userAgent);
            refreshToken.UserId = user.Id;

            user.RefreshTokens?.Add(refreshToken);
            await _userManager.UpdateAsync(user);
            _logger.LogInformation("User {UserName} registered successfully with refresh token.", user.UserName);

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