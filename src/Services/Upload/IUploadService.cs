using System.IO.Pipelines;

using json_api_test.Models.Upload;

namespace json_api_test.Services.Upload;

public interface IUploadService
{
    /// <summary>
    /// Reads the body of the request,
    /// stores the data from the fields to a separate storage
    /// and returns links to the loaded data.
    /// </summary>
    /// <param name="reader">Provides access to a read side of pipe.</param>
    /// <param name="uploadRequestDto">An object containing the names of the fields.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous upload operation.</returns>
    public Task<UploadResponseVm> UploadAsync(
        PipeReader reader, UploadRequestDto uploadRequestDto, CancellationToken cancellationToken = default);
}
