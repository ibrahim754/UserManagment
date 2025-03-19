using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging; // For DbUpdateException (EF Core)

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database update error occurred");
            await HandleExceptionAsync(context, ex, StatusCodes.Status400BadRequest);
        }
        catch (ServerException ex) // Replace with your custom exception
        {
            _logger.LogError(ex, "Server error occurred");
            await HandleExceptionAsync(context, ex, StatusCodes.Status500InternalServerError);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex, StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception,
        int statusCode)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = new
        {
            Status = statusCode,
            Message = exception.Message,
            Details = context.Response.StatusCode.ToString(),
            // Only include stack trace in development environment
            StackTrace = context.RequestServices.GetService<IWebHostEnvironment>().IsDevelopment()
                ? exception.StackTrace
                : null
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}

// Custom exception example
public class ServerException : Exception
{
    public ServerException(string message) : base(message)
    {
    }
}