using Microsoft.Extensions.Options;

namespace json_api_test.Middleware.Validations;

internal class ContentTypeValidator
{
    internal class ValidationOptions
    {
        public IEnumerable<string> ContentTypes { get; set; } = Array.Empty<string>();
    }

    private readonly RequestDelegate _next;
    private readonly IOptions<ValidationOptions> _options;

    public ContentTypeValidator(RequestDelegate next, IOptions<ValidationOptions> options)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var acceptedTypes = _options.Value.ContentTypes;
        if (acceptedTypes.Any())
        {
            var contentType = context.Request.ContentType;
            if (string.IsNullOrEmpty(contentType) || !acceptedTypes.Any(x => contentType.Contains(x)))
            {
                throw new ContentTypeValidationException();
            }
        }

        await _next(context);
    }
}
