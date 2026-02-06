using Serilog.Context;

namespace Nexus.DEB.Api.Security
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private const string CorrelationIdHeaderName = "X-Correlation-ID";

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Check if correlation ID was passed in (e.g., from a parent service)
            // Otherwise generate a new one
            var correlationId = context.Request.Headers[CorrelationIdHeaderName].FirstOrDefault()
                                ?? Guid.NewGuid().ToString("D");

            // Store in HttpContext.Items for access throughout the request
            context.Items["CorrelationId"] = correlationId;

            // Add to response headers so clients can see it
            context.Response.OnStarting(() =>
            {
                context.Response.Headers.TryAdd(CorrelationIdHeaderName, correlationId);
                return Task.CompletedTask;
            });

            // Push to Serilog's LogContext - all logs within this scope will include it
            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                await _next(context);
            }
        }
    }
}
