using ErrorOr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Framework;
using Microsoft.Extensions.Logging;

namespace UserManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BaseController : ControllerBase
    {
           
        protected IActionResult Problem(IEnumerable<Error> errors)
        {
            if (errors == null || !errors.Any())
            {
                // Return a generic 500 error if there are no specific errors to handle.
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }

            // Use the error with the highest priority to determine the status code.
            var firstError = errors.First();
            var statusCode = GetStatusCodeForErrorType(firstError.Type);

            // If there are multiple errors, return them as a collection in the response.
            var errorDetails = errors.Select(error => new
            {
                Code = error.Code,
                Description = error.Description
            });

            return StatusCode(statusCode, errorDetails);
        }

        // Common problem method overload for a single error.
        protected IActionResult Problem(Error error)
        {
            var statusCode = GetStatusCodeForErrorType(error.Type);
            return StatusCode(statusCode, new { error.Code, error.Description });
        }

        // Helper method to map ErrorType to standard HTTP status codes.
        private static int GetStatusCodeForErrorType(ErrorType errorType)
        {
            return errorType switch
            {
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.Failure => StatusCodes.Status500InternalServerError,
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                ErrorType.Forbidden => StatusCodes.Status403Forbidden,
                _ => StatusCodes.Status500InternalServerError // Default to 500 if type is unknown
            };
        }
       
    }
}
