namespace json_api_test.Services.Storage;

public class StorageService : IStorage
{
    private FileStream _writer;
    private readonly Dictionary<string, string> _uploadLocations = new();

    public async Task UploadAsync(
        byte[] buffer, string uploadToken, bool isFinalBlock, CancellationToken cancellationToken = default)
    {
        await UploadAsync(buffer, 0, buffer.Length, uploadToken, isFinalBlock, cancellationToken);
    }

    public async Task UploadAsync(
        byte[] buffer, int start, int length, string uploadToken, bool isFinalBlock, CancellationToken cancellationToken = default)
    {
        const int BufferSize1M = 1024 * 1024;

        if (!_uploadLocations.ContainsKey(uploadToken))
        {
            //var filename = System.IO.Path.GetTempFileName();
            await CloseWriterAsync().ConfigureAwait(false);
            var filename = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString());
            _writer = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None, BufferSize1M);
            _uploadLocations.Add(uploadToken, filename);
        }

        try
        {
            await _writer.WriteAsync(buffer.AsMemory(start, length), cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
        }
        catch
        {
            await CloseWriterAsync().ConfigureAwait(false);
            foreach (var filename in _uploadLocations.Values)
            {
                if (System.IO.File.Exists(filename))
                {
                    System.IO.File.Delete(filename);
                }
            }
            throw;
        }

        if (isFinalBlock)
        {
            await CloseWriterAsync().ConfigureAwait(false);
        }
    }

    public string this[string uploadToken] => _uploadLocations[uploadToken];

    private async Task CloseWriterAsync()
    {
        if (_writer != null)
        {
            await _writer.DisposeAsync().ConfigureAwait(false);
            _writer = null!;
        }
    }
}
