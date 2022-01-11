using Microsoft.AspNetCore.Mvc;

using json_api_test.Models.Upload;
using json_api_test.Services.Upload;

namespace json_api_test.Controllers;

[ApiController]
[Route("api/test/[action]")]
public class TestController : ControllerBase
{
    private readonly IUploadService _uploadService;

    public TestController(IUploadService uploadService)
    {
        _uploadService = uploadService ?? throw new ArgumentNullException(nameof(uploadService));
    }

    [RequestSizeLimit(1_000_000_000)]
    [HttpPost]
    public async Task<IActionResult> Upload(CancellationToken cancellationToken)
    {
        var vm = await _uploadService.UploadAsync(
            Request.BodyReader, new UploadRequestDto(), cancellationToken);

        return Ok(vm);
    }
}
