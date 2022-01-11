using json_api_test.Middleware.Exceptions;
using json_api_test.Middleware.Validations;

using Microsoft.Extensions.Options;

namespace json_api_test.Middleware;

internal static class MiddlewareExtensions
{
    public static IApplicationBuilder UseCustomExceptionHandler(
        this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CustomExceptionHandler>();
    }

    public static IApplicationBuilder UseContentTypeValidator(
        this IApplicationBuilder builder, IOptions<ContentTypeValidator.ValidationOptions> options)
    {
        return builder.UseMiddleware<ContentTypeValidator>(options);
    }
}
