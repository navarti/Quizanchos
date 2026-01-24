using Quizanchos.Common.Util;
using System.Text.Json;

namespace Quizanchos.WebApi.Extensions;

public class ExceptionMiddlewareExtension
{
    private readonly RequestDelegate _next;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionMiddlewareExtension"/> class.
    /// </summary>
    /// <param name="next">Next delegate.</param>
    public ExceptionMiddlewareExtension(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context).ConfigureAwait(false);
        }
        catch (UnauthorizedAccessException ex)
        {
            string messageForUser = ex.Message;
            await HandleExceptionAsync(context, messageForUser, StatusCodes.Status403Forbidden).ConfigureAwait(false);
        }
        catch (QuizanchosException ex)
        {
            string messageForUser = ex.Message;
            await HandleExceptionAsync(context, messageForUser, StatusCodes.Status400BadRequest).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);

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