using System.Net;
using System.Text.Json;
using Artisaback.Api.Middleware;
using FluentAssertions;
using Microsoft.AspNetCore.Http;

namespace Artisaback.Tests.Api.Middlewares;

public class ExceptionMiddlewareTests
{
    [Fact]
    public async Task Invoke_ShouldReturn_Unauthorized_WhenUnauthorizedAccessExceptionIsThrown()
    {
        // Arrange
        RequestDelegate next = context => throw new UnauthorizedAccessException("Test error message");
        var middleware = new ExceptionMiddleware(next);
        var context = new DefaultHttpContext();
        var memoryStream = new MemoryStream();
        context.Response.Body = memoryStream;

        // Act
        await middleware.Invoke(context);
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();

        // Assert
        // Because System.Text.Json defaults to camelCase, we check for "error"
        var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseBody);
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.Unauthorized);
        jsonResponse.GetProperty("error").GetString().Should().Be("Test error message");
    }
}