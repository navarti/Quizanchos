using Quizanchos.Common.Util;
using System.Text.Json;

namespace Quizanchos.WebApi.Extensions;

public class ExceptionMiddlewareExtension
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddlewareExtension> _logger;

    public ExceptionMiddlewareExtension(RequestDelegate next, ILogger<ExceptionMiddlewareExtension> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context).ConfigureAwait(false);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access on {Method} {Path}", context.Request.Method, context.Request.Path);
            await HandleExceptionAsync(context, ex.Message, StatusCodes.Status403Forbidden).ConfigureAwait(false);
        }
        catch (QuizanchosException ex)
        {
            _logger.LogWarning(ex, "Domain exception on {Method} {Path}", context.Request.Method, context.Request.Path);
            await HandleExceptionAsync(context, ex.Message, StatusCodes.Status400BadRequest).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception on {Method} {Path}", context.Request.Method, context.Request.Path);

            string messageForUser = "Internal Server Error. Please try again later or contact support.";
            await HandleExceptionAsync(context, messageForUser, StatusCodes.Status500InternalServerError).ConfigureAwait(false);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, string messageForUser, int statusCode)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        return context.Response.WriteAsync(JsonSerializer.Serialize(new
        {
            Message = messageForUser,
        }));
    }
}
