using CloudinaryDotNet;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Quartz;
using System.Text;
using UserManagement.DAL;
using UserManagement.DAL.Configurations.Quartz;
using UserManagement.Entites;
using UserManagement.Interfaces;
using UserManagement.Models;
using UserManagement.Seeding;
using UserManagement.Services;

namespace UserManagement.Extensions
{
    public static class UserManagementServiceExtension
    {
        public static IServiceCollection AddUserManagementServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddExcpetionFilter(configuration);
            services.AddLogging(configuration);
            services.AddSignalRDI();
            services.AddIdentityServices();
            services.AddDbContext(configuration);
            services.ConfigureJwtAuthentication(configuration);
            services.AddCloudinary(configuration);
            services.AddUserManagementDependencies();
            services.AddEmailConfiguration(configuration);
            //services.AddPendingRegistrationConfiguration();
            services.AddQuartzDI(configuration);
            services.AddCacheConfiguration();
            services.AddSeedingDI(configuration);
            return services;
        }

        private static IServiceCollection AddLogging(this IServiceCollection services, IConfiguration configuration)
        {
            //services.AddMemoryCache();
            return services;
        }
        private static IServiceCollection AddExcpetionFilter(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers(options =>
            {
                options.Filters.Add<GlobalExceptionFilter>();
            });
            return services;
        }
        private static IServiceCollection AddSignalRDI(this IServiceCollection services )
        {
            services.AddSignalR();
            return services;
        }

        private static IServiceCollection AddQuartzDI(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddQuartz(options =>
            {

            }
            );
            services.AddQuartzHostedService(options =>
                    options.WaitForJobsToComplete = true
                );
            services.ConfigureOptions<RefreshTokenBackgroundJobSetup>();
            return services;
        }

        private static IServiceCollection AddIdentityServices(this IServiceCollection services)
        {
            services.AddIdentity<User, IdentityRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
            })
            .AddEntityFrameworkStores<UserManagmentDbContext>()
            .AddDefaultTokenProviders();

            services.Configure<SecurityStampValidatorOptions>(options =>
            {
                options.ValidationInterval = TimeSpan.Zero;  
            });

            return services;
        }

        private static IServiceCollection AddDbContext(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<UserManagmentDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions => sqlOptions.MigrationsAssembly("Web")));

            //services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            return services;
        }     
        private static IServiceCollection AddSeedingDI(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<IDataSeeder, RoleSeeder>();
            services.AddTransient<IDataSeeder, UsersSeeder>();

             services.AddHostedService<DataSeedingRunner>();
            return services;
        }

        private static IServiceCollection ConfigureJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<JWT>(configuration.GetSection("JWT"));

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(o =>
            {
                o.RequireHttpsMetadata = false;
                o.SaveToken = false;
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidIssuer = configuration["JWT:Issuer"],
                    ValidAudience = configuration["JWT:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Key"])),
                    ClockSkew = TimeSpan.Zero
                };
            });

            return services;
        }

        private static IServiceCollection AddCloudinary(this IServiceCollection services, IConfiguration configuration)
        {
            var cloudinaryConfig = configuration.GetSection("Cloudinary");

            services.AddSingleton(provider =>
            {
                var account = new Account(
                    cloudinaryConfig["CloudName"],
                    cloudinaryConfig["ApiKey"],
                    cloudinaryConfig["ApiSecret"]
                );

                return new Cloudinary(account);
            });

            services.AddHttpClient();
            return services;
        }

        private static IServiceCollection AddUserManagementDependencies(this IServiceCollection services)
        {
            services.AddScoped<ICloudinaryService, CloudinaryService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IFormateService, FormateService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IRegistrationService, RegistrationService>();
            services.AddScoped<IUserManagementService, UserManagementService>();
            services.AddTransient<RefreshTokenBackgroundJob>();
 
            return services;
        }
        private static IServiceCollection AddEmailConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<MailSettings>(configuration.GetSection("MailSettings"));
            services.AddTransient<IMailService, MailService>();
            return services;
        }
        //private static IServiceCollection AddPendingRegistrationConfiguration(this IServiceCollection services)
        //{
        //    services.AddSingleton<IPendingRegistrationService, PendingRegistrationService>();
        //    return services;
        //}
        private static IServiceCollection AddCacheConfiguration(this IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddScoped<ICacheService,CacheService>();
            return services;

        }
    }
}
