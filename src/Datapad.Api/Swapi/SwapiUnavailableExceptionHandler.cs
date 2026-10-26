using Microsoft.AspNetCore.Diagnostics;

namespace Datapad.Api.Swapi;

/// <summary>Turns an upstream outage into a 503 problem response the client can show as "try again".</summary>
internal sealed class SwapiUnavailableExceptionHandler(IProblemDetailsService problemDetails) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not SwapiUnavailableException)
        {
            return false;
        }

        httpContext.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails =
            {
                Status = StatusCodes.Status503ServiceUnavailable,
                Title = "Star Wars API unavailable",
                Detail = "The Star Wars API (swapi.info) could not be reached. Please try again shortly.",
            },
        });
    }
}
