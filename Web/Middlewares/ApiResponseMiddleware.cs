using ErrorOr;
using Newtonsoft.Json;

public class ApiResponseMiddleware
{
    private readonly RequestDelegate _next;

    public ApiResponseMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        // Capture the original response body
        var originalResponseBodyStream = context.Response.Body;

        try
        {
            using var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            await _next(context); // Proceed with the request pipeline

            // Read the response content
            memoryStream.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(memoryStream).ReadToEndAsync();

            object? result;
            if (context.Response.StatusCode >= 400)
            {
                // Handle error scenarios (HTTP status codes >= 400)
                var errorMessage = "An unexpected error occurred.";
                result = new ApiResponse<object>(errorMessage)
                {
                    ErrorCode = context.Response.StatusCode.ToString()
                };
            }
            else
            {
                // Deserialize the original response
                var responseType = typeof(object);
                result = JsonConvert.DeserializeObject(responseBody);

                // Wrap the response
                if (result is ErrorOr<object> errorOr && errorOr.IsError)
                {
                    var errorMessage = errorOr.Errors.FirstOrDefault().Description ?? "An error occurred.";
                    result = new ApiResponse<object>(errorMessage);
                }
                else
                {
                    result = new ApiResponse<object>(result);
                }
            }

            // Rewrite the response with the wrapped object
            context.Response.Body = originalResponseBodyStream;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonConvert.SerializeObject(result));
        }
        catch (Exception ex)
        {
            context.Response.Body = originalResponseBodyStream;
            context.Response.StatusCode = 500; // Internal Server Error
            await context.Response.WriteAsync(JsonConvert.SerializeObject(new ApiResponse<object>(
                ex.Message, "500")));
        }
    }
}
