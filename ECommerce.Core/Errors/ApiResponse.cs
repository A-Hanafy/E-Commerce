namespace ECommerce.Core.Errors;

public class ApiResponse
{
    public ApiResponse(int statusCode, string? message = null)
    {
        StatusCode = statusCode;
        Message = message ?? GetDefaultMessageForStatusCode(statusCode);
    }

    public int StatusCode { get; set; }
    public string? Message { get; set; }

    private static string? GetDefaultMessageForStatusCode(int statusCode)
    {
        return statusCode switch
        {
            400 => "A bad request was made.",
            401 => "You are not authorized.",
            403 => "Forbidden from accessing this resource.",
            404 => "Resource not found.",
            500 => "An unexpected error occurred on the server.",
            _ => null
        };
    }
}
