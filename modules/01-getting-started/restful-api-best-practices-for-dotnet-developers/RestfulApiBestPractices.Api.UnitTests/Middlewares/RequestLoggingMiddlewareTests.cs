using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using RestfulApiBestPractices.Api.Middlewares;
using Xunit;
using AwesomeAssertions;

namespace RestfulApiBestPractices.Api.UnitTests.Middlewares;

public class RequestLoggingMiddlewareTests
{
    [Fact]
    public async Task RequestLoggingMiddleware_shoudCallNext()
    {
        var context = new DefaultHttpContext();
        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };

        var middleware = new RequestLoggingMiddleware(next, NullLogger<RequestLoggingMiddleware>.Instance);
        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
    }
}