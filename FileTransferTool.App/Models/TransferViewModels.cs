namespace FileTransferTool.App.Models;

internal enum TransferDirection
{
    Send,
    Receive
}

internal enum TransferStage
{
    Queued,
    Running,
    Succeeded,
    Failed
}

internal sealed class TransferSnapshot
{
    // 传输快照是运行时到界面的只读消息，界面只消费，不反向修改。
    public Guid Id { get; init; }
    public string LinkName { get; init; } = string.Empty;
    public TransferDirection Direction { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public long BytesTransferred { get; init; }
    public long TotalBytes { get; init; }
    public TransferStage Stage { get; init; }
    public string Message { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
}
