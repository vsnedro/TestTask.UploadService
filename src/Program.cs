using System.Net.Mime;

using json_api_test.Controllers;
using json_api_test.Middleware;
using json_api_test.Middleware.Validations;
using json_api_test.Services.Storage;
using json_api_test.Services.Upload;

using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers();

builder.Services
    .AddScoped<IStorage, StorageService>()
    .AddScoped<IUploadService, UploadService>();

var app = builder.Build();

app.UseCustomExceptionHandler();
app.UseContentTypeValidator(Options.Create(
    new ContentTypeValidator.ValidationOptions
    {
        ContentTypes = new string[] { MediaTypeNames.Application.Json }
    }));
app.UseAuthorization();

app.MapControllers();

app.Run();
