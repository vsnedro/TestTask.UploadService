namespace json_api_test.Middleware.Validations;

internal class ContentTypeValidationException : Exception
{
    public ContentTypeValidationException()
    {
    }

    public ContentTypeValidationException(string message)
        : base(message)
    {
    }

    public ContentTypeValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
