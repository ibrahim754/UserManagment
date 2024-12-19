public class ApiResponse<T>
{
    public bool IsSucceeded { get; set; }
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorCode { get; set; }

    // Success constructor
    public ApiResponse(T data)
    {
        IsSucceeded = true;
        Data = data;
    }

    // Error constructor
    public ApiResponse(string errorMessage, string? errorCode = null)
    {
        IsSucceeded = false;
        ErrorMessage = errorMessage;
        ErrorCode = errorCode;
    }
}
