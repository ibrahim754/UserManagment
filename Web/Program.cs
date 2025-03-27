using UserManagement.Extensions;
using Web.Extensions;

namespace Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            // Add SeriLogConfig
   
          

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddCors(options => {
                options.AddPolicy("AllowAll", policy => {
                    policy.AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowAnyOrigin()
                          .WithExposedHeaders("SignalR-Status"); // For SignalR
                });
            });
            // Register UserManagementService 
            builder.Services.AddUserManagementServices(builder.Configuration);
       
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerService();
            var app = builder.Build();
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            //app.UseMiddleware<ExceptionHandlingMiddleware>();
            app.UseCors(); 
            //app.UseMiddleware<ApiResponseMiddleware>();
            app.UseHttpsRedirection();

            app.UseAuthorization();
 
            app.MapControllers();
            app.ConfigureApplication();
            app.Run();
        }
    }
}
