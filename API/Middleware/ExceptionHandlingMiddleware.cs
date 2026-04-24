using API.Models;
using Domain.Exceptions;

namespace API.Middleware
{
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var (statusCode, message, errorCode, details) = exception switch
            {
                BaseException ex => (ex.StatusCode, ex.Message, ex.ErrorCode, ex.Details),
                _ => (500, "An unexpected error occurred", "INTERNAL_ERROR", (object?)null)
            };

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(new ErrorResponse
            {
                StatusCode = statusCode,
                Message = message,
                ErrorCode = errorCode,
                Details = details,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}
