using System.Net.Sockets;
using System.Text.Json;

namespace FileTransferTool.App.Services;

internal static class TransferProtocol
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    // 控制消息允许较长超时，兼顾大文件传输时网络抖动和弱网环境。
    private static readonly TimeSpan DefaultMessageTimeout = TimeSpan.FromMinutes(5);

    // 控制类消息统一编码成 envelope，便于区分消息类型和消息体。
    public static async Task WriteMessageAsync<T>(NetworkStream stream, string type, T payload, CancellationToken cancellationToken)
    {
        // 所有控制消息统一走“长度前缀 + JSON 包体”，便于后续扩展协议字段。
        var envelope = new ProtocolEnvelope
        {
            Type = type,
            Payload = JsonSerializer.SerializeToElement(payload, JsonOptions)
        };

        var bytes = JsonSerializer.SerializeToUtf8Bytes(envelope, JsonOptions);
        var length = BitConverter.GetBytes(bytes.Length);
        await WaitWithTimeoutAsync(stream.WriteAsync(length, cancellationToken), cancellationToken).ConfigureAwait(false);
        await WaitWithTimeoutAsync(stream.WriteAsync(bytes, cancellationToken), cancellationToken).ConfigureAwait(false);
    }

    // 所有控制消息先收长度，再收消息体，避免 TCP 粘包/拆包影响解析。
    public static async Task<(string Type, T Payload)> ReadMessageAsync<T>(NetworkStream stream, CancellationToken cancellationToken)
    {
        var header = await ReadExactAsync(stream, sizeof(int), cancellationToken).ConfigureAwait(false);
        var length = BitConverter.ToInt32(header, 0);
        var body = await ReadExactAsync(stream, length, cancellationToken).ConfigureAwait(false);
        var envelope = JsonSerializer.Deserialize<ProtocolEnvelope>(body, JsonOptions)
            ?? throw new IOException("协议消息为空");
        var payload = envelope.Payload.Deserialize<T>(JsonOptions)
            ?? throw new IOException("协议消息解析失败");
        return (envelope.Type, payload);
    }

    public static async Task WriteChunkAsync(NetworkStream stream, FileChunk chunk, CancellationToken cancellationToken)
    {
        // 大文件内容单独发送原始字节，避免把二进制塞进 JSON 造成额外开销。
        await WriteMessageAsync(stream, MessageTypes.ChunkHeader, new ChunkHeader
        {
            Offset = chunk.Offset,
            Length = chunk.Length
        }, cancellationToken).ConfigureAwait(false);
        await WaitWithTimeoutAsync(stream.WriteAsync(chunk.Buffer.AsMemory(0, chunk.Length), cancellationToken), cancellationToken).ConfigureAwait(false);
    }

    public static async Task<FileChunk> ReadChunkAsync(NetworkStream stream, CancellationToken cancellationToken)
    {
        var (type, header) = await ReadMessageAsync<ChunkHeader>(stream, cancellationToken).ConfigureAwait(false);
        if (!string.Equals(type, MessageTypes.ChunkHeader, StringComparison.Ordinal))
        {
            throw new IOException($"协议错误，期待 {MessageTypes.ChunkHeader}，实际 {type}");
        }

        var buffer = await ReadExactAsync(stream, header.Length, cancellationToken).ConfigureAwait(false);
        return new FileChunk(buffer, header.Offset, header.Length);
    }

    private static async Task<byte[]> ReadExactAsync(NetworkStream stream, int length, CancellationToken cancellationToken)
    {
        var buffer = new byte[length];
        var offset = 0;
        while (offset < length)
        {
            // 网络流不保证一次读满，这里必须循环直到读够指定长度。
            var read = await WaitWithTimeoutAsync(stream.ReadAsync(buffer.AsMemory(offset, length - offset), cancellationToken), cancellationToken).ConfigureAwait(false);
            if (read == 0)
            {
                throw new IOException("连接已中断");
            }

            offset += read;
        }

        return buffer;
    }

    private static async Task WaitWithTimeoutAsync(ValueTask task, CancellationToken cancellationToken)
    {
        await task.AsTask().WaitAsync(DefaultMessageTimeout, cancellationToken).ConfigureAwait(false);
    }

    // 统一把 ValueTask<T> 包成带超时的 Task<T>，便于处理假死连接。
    private static async Task<T> WaitWithTimeoutAsync<T>(ValueTask<T> task, CancellationToken cancellationToken)
    {
        return await task.AsTask().WaitAsync(DefaultMessageTimeout, cancellationToken).ConfigureAwait(false);
    }
}

internal static class MessageTypes
{
    public const string Start = "start";
    public const string Resume = "resume";
    public const string ChunkHeader = "chunk";
    public const string Ack = "ack";
    public const string Complete = "complete";
    public const string Error = "error";
}

internal sealed class ProtocolEnvelope
{
    public string Type { get; set; } = string.Empty;
    public JsonElement Payload { get; set; }
}

internal sealed class StartFileRequest
{
    public string LinkIdentity { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string RelativePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public int ChunkSize { get; set; }
    public long LastWriteUtcTicks { get; set; }
}

internal sealed class ResumeResponse
{
    public long Offset { get; set; }
    public string Message { get; set; } = string.Empty;
}

internal sealed class ChunkHeader
{
    public long Offset { get; set; }
    public int Length { get; set; }
}

internal sealed class AckResponse
{
    public long NextOffset { get; set; }
}

internal sealed class CompleteRequest
{
    public string FileName { get; set; } = string.Empty;
}

internal sealed class ErrorResponse
{
    public string Message { get; set; } = string.Empty;
}

internal readonly record struct FileChunk(byte[] Buffer, long Offset, int Length);
