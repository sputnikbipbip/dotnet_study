namespace RestfulApiBestPractices.Api.Middlewares;

public class CorrelationIdMiddleware(ILogger<CorrelationIdMiddleware> logger) : IMiddleware
{
    private const string CorrelationIdHeader = "X-Correlation-Id";

    /*
     Use convention-based middleware (the default) when:

       Your middleware only needs singleton services (like ILogger, IConfiguration)
       You do not need scoped services from the DI container
       You want simpler registration (no DI registration needed)
    */
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault()
                            ?? Guid.NewGuid().ToString();

        context.Items["CorrelationId"] = correlationId;
        context.Response.Headers[CorrelationIdHeader] = correlationId;

        using (logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
        {
            logger.LogInformation("Request {Path} assigned CorrelationId {CorrelationId}",
                context.Request.Path, correlationId);
            await next(context);
        }
    }
}