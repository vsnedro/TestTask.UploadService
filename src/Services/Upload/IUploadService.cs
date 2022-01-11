using System.IO.Pipelines;

using json_api_test.Models.Upload;

namespace json_api_test.Services.Upload;

public interface IUploadService
{
    public Task<UploadResponseVm> UploadAsync(
        PipeReader reader, UploadRequestDto uploadRequestDto, CancellationToken cancellationToken = default);
}
