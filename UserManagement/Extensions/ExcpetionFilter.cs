using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace UserManagement.Extensions
{
    public class GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
        : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            var exception = context.Exception;
            var exceptionType = exception.GetType().Name;
            var message = exception.Message;
            var stackTrace = exception.StackTrace;

            // Log the error with Serilog
            logger.LogError(exception, "Exception: {ExceptionType}, Message: {Message}, StackTrace: {StackTrace}",
                exceptionType, message, stackTrace);

            // Customize the response
            var problemDetails = new ProblemDetails
            {
                Status = (int)HttpStatusCode.InternalServerError,
                Title = "An unexpected error occurred",
                Detail = message,
                Extensions = { { "ExceptionType", exceptionType } }
            };

            context.Result = new ObjectResult(problemDetails)
            {
                StatusCode = (int)HttpStatusCode.InternalServerError
            };

            // Mark the exception as handled
            context.ExceptionHandled = true;
        }
    }
}