using ModularMonolith.Shared.Common;
using Microsoft.AspNetCore.Diagnostics;

namespace ModularMonolith.Api.Middleware
{
    public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            logger.LogError(exception, "Unhandled exception occurred");

            const string message = "An unexpected error occurred. Please contact support if the problem persists.";

            var response = Result.Failure(ErrorType.Failure, message);

            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            httpContext.Response.ContentType = "application/json";

            await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

            return true;
        }
    }
}
