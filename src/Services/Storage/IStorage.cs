namespace json_api_test.Services.Storage;

public interface IStorage
{
    /// <summary>
    /// Asynchronously uploads a sequence of bytes and monitors cancellation requests.
    /// </summary>
    /// <param name="buffer">The buffer to upload data from.</param>
    /// <param name="uploadToken">Unique upload token.</param>
    /// <param name="isFinalBlock">
    /// true if the input buffer contains the entire data to upload.
    /// false if the input buffer contains partial data with more data to follow.
    /// </param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous upload operation.</returns>
    public Task UploadAsync(
        byte[] buffer, string uploadToken, bool isFinalBlock, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously uploads a sequence of bytes and monitors cancellation requests.
    /// </summary>
    /// <param name="buffer">The buffer to upload data from.</param>
    /// <param name="start">The index from which to begin uploading bytes.</param>
    /// <param name="length">The maximum number of bytes to upload.</param>
    /// <param name="uploadToken">Unique upload token.</param>
    /// <param name="isFinalBlock">
    /// true if the input buffer contains the entire data to upload.
    /// false if the input buffer contains partial data with more data to follow.
    /// </param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous upload operation.</returns>
    public Task UploadAsync(
        byte[] buffer, int start, int length, string uploadToken, bool isFinalBlock, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets upload location by upload token.
    /// </summary>
    /// <param name="uploadToken">Unique upload token.</param>
    /// <returns>Upload location by upload token.</returns>
    public string this[string uploadToken] { get; }
}
