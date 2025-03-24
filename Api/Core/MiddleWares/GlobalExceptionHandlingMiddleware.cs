using Microsoft.IdentityModel.Tokens;

public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
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
        catch (Exception ex)
        {
            // Log the exception (optional)
            _logger.LogError(ex, "An unhandled exception occurred.");

            // Handle specific exceptions (like token expiration)
            if (ex is SecurityTokenExpiredException)
            {
                await HandleTokenExpiredExceptionAsync(context, (SecurityTokenExpiredException)ex);
            }
            else
            {
                await HandleGlobalExceptionAsync(context, ex);
            }
        }
    }

    private Task HandleTokenExpiredExceptionAsync(HttpContext context, SecurityTokenExpiredException ex)
    {
        context.Response.StatusCode = 401;
        context.Response.ContentType = "application/json";

        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            StatusCode = 401,
            Message = "Your token has expired. Please login again."
        });

        return context.Response.WriteAsync(result);
    }

    private Task HandleGlobalExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            StatusCode = 500,
            Message = "An unexpected error occurred. Please try again later.",
            Detailed = ex.Message 
        });

        return context.Response.WriteAsync(result);
    }
}
