using System.Diagnostics;
using System.Net;
using System.Net.Mime;
using System.Text.Json;

using json_api_test.Middleware.Validations;

using Microsoft.AspNetCore.Connections;

namespace json_api_test.Middleware.Exceptions;

internal class CustomExceptionHandler
{
    private readonly RequestDelegate _next;

    public CustomExceptionHandler(RequestDelegate next) =>
        _next = next ?? throw new ArgumentNullException(nameof(next));

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
#if DEBUG
            Debug.WriteLine($"Exception: {ex.GetType()}: {ex.Message}");
#endif
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var responseCode = HttpStatusCode.InternalServerError;
        var responseText = string.Empty;

        switch (exception)
        {
            case ContentTypeValidationException:
                responseCode = HttpStatusCode.UnsupportedMediaType;
                break;

            case OperationCanceledException:
            case ConnectionResetException:
                responseCode = HttpStatusCode.NoContent;
                break;

            default:
                responseText = JsonSerializer.Serialize(new { error = exception.Message });
                break;
        }

        context.Response.ContentType = MediaTypeNames.Application.Json;
        context.Response.StatusCode = (int)responseCode;
        await context.Response.WriteAsync(responseText);
    }
}
