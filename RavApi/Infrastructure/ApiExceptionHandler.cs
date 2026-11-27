using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace RavApi.Infrastructure;

public sealed class ApiExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ApiExceptionHandler> _logger;

    public ApiExceptionHandler(ILogger<ApiExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        string requestId = Guid.TryParse(httpContext.Request.Headers["X-RAV-Request-Id"].ToString(), out Guid suppliedRequestId)
            ? suppliedRequestId.ToString("N")
            : httpContext.TraceIdentifier;
        string flowId = Guid.TryParse(httpContext.Request.Headers["X-RAV-Flow-Id"].ToString(), out Guid suppliedFlowId)
            ? suppliedFlowId.ToString("N")
            : "-";
        string route = httpContext.GetEndpoint() is Microsoft.AspNetCore.Routing.RouteEndpoint routeEndpoint
            ? routeEndpoint.RoutePattern.RawText ?? "unknown"
            : "unmatched";

        _logger.LogError(
            exception,
            "Unhandled API exception. Route={Route} RequestId={RequestId} FlowId={FlowId} TraceId={TraceId}",
            route,
            requestId,
            flowId,
            httpContext.TraceIdentifier);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await Results.Problem(
            statusCode: StatusCodes.Status500InternalServerError,
            title: "An unexpected error occurred.",
            detail: "The request could not be completed.")
            .ExecuteAsync(httpContext);

        return true;
    }
}
