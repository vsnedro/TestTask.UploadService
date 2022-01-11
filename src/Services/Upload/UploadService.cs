using System.Buffers;
using System.Buffers.Text;
using System.Collections.Concurrent;
using System.IO.Pipelines;
using System.Text;

using json_api_test.Models.Upload;
using json_api_test.Services.Storage;

namespace json_api_test.Services.Upload;

public class UploadService : IUploadService
{
    private const int MaxBlockingCollectionBoundedCapacity = 50;
    private const int FromBase64InputBlockSize = 4;

    private readonly IStorage _storage;

    public UploadService(IStorage storage)
    {
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));
    }

    /// <summary>
    /// Reads the body of the request,
    /// stores the data from the fields to a separate storage
    /// and returns links to the loaded data.
    /// </summary>
    public async Task<UploadResponseVm> UploadAsync(
        PipeReader reader, UploadRequestDto requestDto, CancellationToken cancellationToken = default)
    {
        _ = reader ?? throw new ArgumentNullException(nameof(reader));
        _ = requestDto ?? throw new ArgumentNullException(nameof(requestDto));

        var vm = new UploadResponseVm();

        using var taskFaultedCts = new CancellationTokenSource();
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, taskFaultedCts.Token);

        using var requestChunks = new BlockingCollection<UploadChunk>(MaxBlockingCollectionBoundedCapacity);
        using var storedChunks = new BlockingCollection<UploadChunk>(MaxBlockingCollectionBoundedCapacity);

        var token = linkedCts.Token;
        var readRequestBodyTask = Task.Run(async () => await ReadRequestBodyAsync(
            reader, requestChunks, requestDto, vm, token), token);
        var decodeRequestBodyTask = Task.Run(async () => await DecodeRequestBody(
           requestChunks, storedChunks, token), token);
        var uploadRequestBodyAsync = Task.Run(async () => await UploadRequestBodyAsync(
            storedChunks, _storage, requestDto, vm, token), token);

        var tasks = new List<Task>()
        {
            readRequestBodyTask, decodeRequestBodyTask, uploadRequestBodyAsync
        };

        try
        {
            while (tasks.Any())
            {
                var task = await Task.WhenAny(tasks);
                if (task.IsCompletedSuccessfully)
                {
                    tasks.Remove(task);
                }
                else
                {
                    if (!taskFaultedCts.IsCancellationRequested)
                    {
                        taskFaultedCts.Cancel();
                    }
                    if (task.IsCanceled)
                    {
                        throw new OperationCanceledException();
                    }
                    throw task.Exception?.InnerException ?? new InvalidOperationException();
                }
            }
        }
        finally
        {
            ClearBlockingCollection(requestChunks);
            ClearBlockingCollection(requestChunks);
        }

        return vm;
    }

    /// <summary>
    /// Reads the body of the request
    /// and adds chunks of data to the thread-safe blocking collection.
    /// </summary>
    private static async Task ReadRequestBodyAsync(
        PipeReader reader, BlockingCollection<UploadChunk> requestChunks, UploadRequestDto requestDto, UploadResponseVm responseVm, CancellationToken cancellationToken = default)
    {
        try
        {
            var jsonObject = new JsonObject();
            while (!cancellationToken.IsCancellationRequested)
            {
                var readResult = await reader.ReadAsync(cancellationToken);
                var buffer = readResult.Buffer;
                try
                {
                    if (readResult.IsCanceled)
                    {
                        break;
                    }

                    var quotesPosition = buffer.PositionOf((byte)'"');
                    do
                    {
                        if (quotesPosition.HasValue && !jsonObject.Key.Started)
                        {
                            jsonObject.Key.Started = true;
                            buffer = buffer.Slice(buffer.GetPosition(1, quotesPosition.Value));
                            // get next quotes position
                            quotesPosition = buffer.PositionOf((byte)'"');
                        }

                        if (quotesPosition.HasValue && !jsonObject.Key.Ended)
                        {
                            jsonObject.Key.Ended = true;
                            jsonObject.Key.Value = Encoding.UTF8.GetString(buffer.Slice(0, quotesPosition.Value));
                            buffer = buffer.Slice(buffer.GetPosition(1, quotesPosition.Value));
                            // get next quotes position
                            quotesPosition = buffer.PositionOf((byte)'"');
                        }

                        if (quotesPosition.HasValue && !jsonObject.Value.Started)
                        {
                            jsonObject.Value.Started = true;
                            buffer = buffer.Slice(buffer.GetPosition(1, quotesPosition.Value));
                            // get next quotes position
                            quotesPosition = buffer.PositionOf((byte)'"');
                        }

                        if (jsonObject.Value.Started && !quotesPosition.HasValue)
                        {
                            switch (jsonObject.Key.Value)
                            {
                                case nameof(requestDto.Json):
                                case nameof(requestDto.File):
                                    var bytesConsumed = buffer.Length - buffer.Length % FromBase64InputBlockSize;
                                    var chunk = new UploadChunk(buffer.Slice(0, bytesConsumed))
                                    {
                                        UploadToken = jsonObject.Key.Value
                                    };
                                    requestChunks.Add(chunk, cancellationToken);
                                    buffer = buffer.Slice(bytesConsumed);
                                    break;
                            }
                        }
                        else if (jsonObject.Value.Started && quotesPosition.HasValue)
                        {
                            switch (jsonObject.Key.Value)
                            {
                                case nameof(requestDto.Name):
                                    responseVm.name = Encoding.UTF8.GetString(buffer.Slice(0, quotesPosition.Value));
                                    break;

                                case nameof(requestDto.Json):
                                case nameof(requestDto.File):
                                    var chunk = new UploadChunk(buffer.Slice(0, quotesPosition.Value))
                                    {
                                        UploadToken = jsonObject.Key.Value,
                                        IsFinal = true
                                    };
                                    requestChunks.Add(chunk, cancellationToken);
                                    break;
                            }

                            jsonObject.Reset();
                            buffer = buffer.Slice(buffer.GetPosition(1, quotesPosition.Value));
                            // get next quotes position
                            quotesPosition = buffer.PositionOf((byte)'"');
                        }
                        else if (!quotesPosition.HasValue)
                        {
                            buffer = buffer.Slice(buffer.End);
                        }
                    } while (quotesPosition.HasValue);

                    if (readResult.IsCompleted)
                    {
                        break;
                    }
                }
                finally
                {
                    reader.AdvanceTo(buffer.Start, buffer.End);
                }
            }
        }
        finally
        {
            requestChunks.CompleteAdding();
            await reader.CompleteAsync();
        }
    }

    /// <summary>
    /// Takes chunks of data from a thread-safe blocking collection,
    /// converts them from base64 format 
    /// and adds to the thread-safe blocking collection.
    /// </summary>
    private static Task DecodeRequestBody(
        BlockingCollection<UploadChunk> encodedChunks, BlockingCollection<UploadChunk> decodedChunks, CancellationToken cancellationToken = default)
    {
        try
        {
            foreach (var chunk in encodedChunks.GetConsumingEnumerable(cancellationToken))
            {
                Base64.DecodeFromUtf8InPlace(chunk.Span, out var bytesWritten);
                chunk.Length = bytesWritten;
                decodedChunks.Add(chunk, cancellationToken);
            }
        }
        finally
        {
            decodedChunks.CompleteAdding();
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Takes chunks of data from a thread-safe blocking collection
    /// and stores them in separate storage.
    /// </summary>
    private static async Task UploadRequestBodyAsync(
        BlockingCollection<UploadChunk> requestChunks, IStorage storage, UploadRequestDto requestDto, UploadResponseVm responseVm, CancellationToken cancellationToken = default)
    {
        foreach (var chunk in requestChunks.GetConsumingEnumerable(cancellationToken))
        {
            using (chunk)
            {
                await storage.UploadAsync(
                    chunk.Data, 0, chunk.Length, chunk.UploadToken!, chunk.IsFinal, cancellationToken);
                switch (chunk.UploadToken)
                {
                    case nameof(requestDto.Json):
                        responseVm.data = storage[chunk.UploadToken];
                        break;
                    case nameof(requestDto.File):
                        responseVm.content = storage[chunk.UploadToken];
                        break;
                }
            }
        }
    }

    private static void ClearBlockingCollection(BlockingCollection<UploadChunk> chunks)
    {
        if (!chunks.IsCompleted)
        {
            if (!chunks.IsAddingCompleted)
            {
                chunks.CompleteAdding();
            }

            foreach (var chunk in chunks)
            {
                chunk.Dispose();
            }
        }
    }
}
