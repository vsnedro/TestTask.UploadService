using System.Buffers;

namespace json_api_test.Models.Upload;

public class UploadChunk : IDisposable
{
    private bool _disposed = false;
    private byte[] _data;

    public string? UploadToken { get; set; }

    public byte[] Data => _data;

    public int Length { get; set; }

    public Span<byte> Span => _data.AsSpan(0, Length);

    public bool IsFinal { get; set; } = false;

    public UploadChunk(int minimumLength)
    {
        _data = ArrayPool<byte>.Shared.Rent(minimumLength);
        Length = minimumLength;
    }

    public UploadChunk(ReadOnlySequence<byte> buffer) : this((int)buffer.Length)
    {
        buffer.CopyTo(_data.AsSpan());
    }

    #region IDisposable
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposeManagedState)
    {
        if (_disposed) return;

        if (disposeManagedState)
        {
            ArrayPool<byte>.Shared.Return(_data);
        }

        _data = null!;
        _disposed = true;
    }
    #endregion
}
