using Microsoft.AspNetCore.Diagnostics;
using ModularMonolith.Shared.Common;

namespace ModularMonolith.Api.Middleware
{
    public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is OperationCanceledException)
            {
                logger.LogInformation("Request was cancelled by the client.");
                return true;
            }

            logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);

            if (httpContext.Response.HasStarted)
            {
                logger.LogWarning("Response has already started, skipping exception handler response.");
                return false;
            }

            const string message = "An unexpected error occurred. Please contact support if the problem persists.";
            var response = Result.Failure(ErrorType.Failure, message);

            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            httpContext.Response.ContentType = "application/json";

            await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

            return true;
        }
    }
}
