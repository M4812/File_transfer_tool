using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using FileTransferTool.App.Models;

namespace FileTransferTool.App.Services;

internal sealed class TransferRuntime : IDisposable
{
    private readonly ConcurrentDictionary<string, SendLinkWorker> _sendWorkers = new(StringComparer.OrdinalIgnoreCase);
    private ReceiverServer? _receiverServer;

    public event Action<string>? LogWritten;
    public event Action<TransferSnapshot>? TransferUpdated;

    // 发送端按链路标识独立启动，每条链路各自监控目录、排队和重试。
    public void StartSend(IEnumerable<SendLinkConfig> links)
    {
        StopSend();
        foreach (var link in links)
        {
            var worker = new SendLinkWorker(link, WriteLog, PublishTransfer);
            if (_sendWorkers.TryAdd(link.LinkIdentity, worker))
            {
                worker.Start();
            }
        }
    }

    public void StopSend()
    {
        foreach (var worker in _sendWorkers.Values)
        {
            worker.Dispose();
        }

        _sendWorkers.Clear();
    }

    // 接收端只有一个监听器，但会根据链路标识把文件分发到不同目标目录。
    public void StartReceive(string ip, int port, IEnumerable<ReceiveLinkConfig> links)
    {
        StopReceive();
        _receiverServer = new ReceiverServer(ip, port, links.ToList(), WriteLog, PublishTransfer);
        _receiverServer.Start();
    }

    public void StopReceive()
    {
        _receiverServer?.Dispose();
        _receiverServer = null;
    }

    public void Dispose()
    {
        StopSend();
        StopReceive();
    }

    private void WriteLog(string message)
    {
        LogWritten?.Invoke(message);
    }

    private void PublishTransfer(TransferSnapshot snapshot)
    {
        TransferUpdated?.Invoke(snapshot);
    }
}

internal sealed class SendLinkWorker : IDisposable
{
    private const int ChunkSize = 1024 * 1024;
    private const int SocketBufferSize = 4 * 1024 * 1024;
    private readonly SendLinkConfig _config;
    private readonly Action<string> _log;
    private readonly Action<TransferSnapshot> _transferUpdated;
    private readonly FileSystemWatcher _watcher;
    private readonly PeriodicTimer _timer = new(TimeSpan.FromSeconds(10));
    private readonly CancellationTokenSource _cts = new();
    private readonly ConcurrentDictionary<string, byte> _pendingFiles = new(StringComparer.OrdinalIgnoreCase);
    private readonly Task _scanTask;

    // 目录监控采用 watcher + 定时扫描双保险，减少高并发文件写入时的漏检。
    public SendLinkWorker(SendLinkConfig config, Action<string> log, Action<TransferSnapshot> transferUpdated)
    {
        _config = config;
        _log = log;
        _transferUpdated = transferUpdated;
        Directory.CreateDirectory(config.MonitorPath);
        _watcher = new FileSystemWatcher(config.MonitorPath)
        {
            IncludeSubdirectories = false,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.Size | NotifyFilters.LastWrite
        };
        _watcher.Created += OnFileChanged;
        _watcher.Changed += OnFileChanged;
        _watcher.Renamed += OnRenamed;
        _scanTask = Task.Run(ProcessLoopAsync);
    }

    public void Start()
    {
        if (!string.IsNullOrWhiteSpace(_config.BackupPath))
        {
            Directory.CreateDirectory(_config.BackupPath);
        }

        _watcher.EnableRaisingEvents = true;
        EnqueueExistingFiles();
        _log($"发送链路[{_config.LinkName}]开始监控 {_config.MonitorPath}");
    }

    public void Dispose()
    {
        _cts.Cancel();
        _watcher.EnableRaisingEvents = false;
        _watcher.Dispose();
        _timer.Dispose();
        try
        {
            _scanTask.Wait(TimeSpan.FromSeconds(2));
        }
        catch
        {
        }
    }

    private async Task ProcessLoopAsync()
    {
        while (!_cts.IsCancellationRequested)
        {
            try
            {
                // 轮询兜底，避免 FileSystemWatcher 在大量事件下漏掉文件。
                foreach (var file in Directory.Exists(_config.MonitorPath)
                    ? Directory.EnumerateFiles(_config.MonitorPath)
                    : Enumerable.Empty<string>())
                {
                    if (ShouldHandle(file))
                    {
                        _pendingFiles.TryAdd(file, 0);
                    }
                }

                foreach (var file in _pendingFiles.Keys.ToArray())
                {
                    if (!_pendingFiles.TryRemove(file, out _))
                    {
                        continue;
                    }

                    if (File.Exists(file) && ShouldHandle(file))
                    {
                        await SendFileWithRetryAsync(file, _cts.Token).ConfigureAwait(false);
                    }
                }

                await _timer.WaitForNextTickAsync(_cts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                _log($"发送链路[{_config.LinkName}]扫描异常: {ex.Message}");
            }
        }
    }

    private async Task SendFileWithRetryAsync(string path, CancellationToken cancellationToken)
    {
        var info = new FileInfo(path);
        var transferId = Guid.NewGuid();
        PublishTransfer(transferId, info.Name, $"{_config.RemoteAddress}:{_config.RemotePort}", 0, info.Exists ? info.Length : 0, TransferStage.Queued, "等待发送");
        for (var attempt = 1; attempt <= 3; attempt++)
        {
            try
            {
                if (!await WaitForFileReadyAsync(path, cancellationToken).ConfigureAwait(false))
                {
                    _pendingFiles.TryAdd(path, 0);
                    return;
                }

                await SendFileAsync(path, transferId, cancellationToken).ConfigureAwait(false);
                BackupFile(path);
                PublishTransfer(transferId, info.Name, $"{_config.RemoteAddress}:{_config.RemotePort}", info.Length, info.Length, TransferStage.Succeeded, "发送成功");
                return;
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                PublishTransfer(transferId, info.Name, $"{_config.RemoteAddress}:{_config.RemotePort}", 0, info.Exists ? info.Length : 0, TransferStage.Failed, ex.Message);
                _log($"发送链路[{_config.LinkName}]发送失败，第{attempt}次重试: {Path.GetFileName(path)}，{ex.Message}");
                await Task.Delay(TimeSpan.FromSeconds(attempt * 2), cancellationToken).ConfigureAwait(false);
            }
        }

        _pendingFiles.TryAdd(path, 0);
    }

    // 真正的发送逻辑保持“连接一次、传完整个文件”，失败时依赖重试和续传恢复。
    private async Task SendFileAsync(string path, Guid transferId, CancellationToken cancellationToken)
    {
        using var client = CreateConfiguredClient();
        await client.ConnectAsync(_config.RemoteAddress, _config.RemotePort, cancellationToken).ConfigureAwait(false);
        await using var stream = client.GetStream();

        var info = new FileInfo(path);
        var request = new StartFileRequest
        {
            LinkIdentity = _config.LinkIdentity,
            FileName = info.Name,
            RelativePath = string.Empty,
            FileSize = info.Length,
            ChunkSize = ChunkSize,
            LastWriteUtcTicks = info.LastWriteTimeUtc.Ticks
        };

        await TransferProtocol.WriteMessageAsync(stream, MessageTypes.Start, request, cancellationToken).ConfigureAwait(false);
        var (_, resume) = await TransferProtocol.ReadMessageAsync<ResumeResponse>(stream, cancellationToken).ConfigureAwait(false);
        // 接收端返回已落盘偏移量，发送端直接从该位置继续读取，实现断点续传。
        var offset = Math.Clamp(resume.Offset, 0, info.Length);
        PublishTransfer(transferId, info.Name, $"{_config.RemoteAddress}:{_config.RemotePort}", offset, info.Length, TransferStage.Running, resume.Message);

        using var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, ChunkSize, FileOptions.SequentialScan);
        file.Seek(offset, SeekOrigin.Begin);
        var currentOffset = offset;
        var buffer = new byte[ChunkSize];
        while (currentOffset < info.Length)
        {
            var read = await file.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken).ConfigureAwait(false);
            if (read <= 0)
            {
                break;
            }

            await TransferProtocol.WriteChunkAsync(stream, new FileChunk(buffer, currentOffset, read), cancellationToken).ConfigureAwait(false);
            var (_, ack) = await TransferProtocol.ReadMessageAsync<AckResponse>(stream, cancellationToken).ConfigureAwait(false);
            // 只有收到确认后才推进偏移，避免断线时误以为远端已经收到。
            currentOffset = ack.NextOffset;
            PublishTransfer(transferId, info.Name, $"{_config.RemoteAddress}:{_config.RemotePort}", currentOffset, info.Length, TransferStage.Running, "发送中");
            file.Seek(currentOffset, SeekOrigin.Begin);
        }

        await TransferProtocol.WriteMessageAsync(stream, MessageTypes.Complete, new CompleteRequest
        {
            FileName = info.Name
        }, cancellationToken).ConfigureAwait(false);

        _log($"发送链路[{_config.LinkName}]成功发送文件: {info.Name}");
    }

    private static TcpClient CreateConfiguredClient()
    {
        var client = new TcpClient
        {
            ReceiveBufferSize = SocketBufferSize,
            SendBufferSize = SocketBufferSize,
            NoDelay = true
        };

        // 大文件传输持续时间长，开启 keep-alive 可以降低中间网络设备误判空闲连接的概率。
        client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
        TrySetTcpKeepAliveOptions(client.Client);
        return client;
    }

    private void BackupFile(string path)
    {
        if (string.IsNullOrWhiteSpace(_config.BackupPath))
        {
            File.Delete(path);
            return;
        }

        var destination = Path.Combine(_config.BackupPath, Path.GetFileName(path));
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        if (File.Exists(destination))
        {
            var stamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            destination = Path.Combine(_config.BackupPath, $"{Path.GetFileNameWithoutExtension(path)}_{stamp}{Path.GetExtension(path)}");
        }

        File.Move(path, destination);
        _log($"发送链路[{_config.LinkName}]已转存到备份目录: {destination}");
    }

    private void EnqueueExistingFiles()
    {
        if (!Directory.Exists(_config.MonitorPath))
        {
            return;
        }

        foreach (var file in Directory.EnumerateFiles(_config.MonitorPath))
        {
            if (ShouldHandle(file))
            {
                _pendingFiles.TryAdd(file, 0);
            }
        }
    }

    private void OnFileChanged(object? sender, FileSystemEventArgs e)
    {
        if (ShouldHandle(e.FullPath))
        {
            _pendingFiles.TryAdd(e.FullPath, 0);
        }
    }

    private void OnRenamed(object? sender, RenamedEventArgs e)
    {
        if (ShouldHandle(e.FullPath))
        {
            _pendingFiles.TryAdd(e.FullPath, 0);
        }
    }

    private bool ShouldHandle(string path)
    {
        if (!File.Exists(path))
        {
            return false;
        }

        var fileName = Path.GetFileName(path);
        if (fileName.EndsWith(".part", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var filters = (_config.FilterType ?? string.Empty)
            .Split([',', ';', '|'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (filters.Length == 0)
        {
            return true;
        }

        var extension = Path.GetExtension(path);
        return filters.Any(item =>
        {
            if (item == "*.*" || item == "*")
            {
                return true;
            }

            var normalized = item.StartsWith('.') ? item : $".{item.TrimStart('*')}";
            return string.Equals(normalized, extension, StringComparison.OrdinalIgnoreCase);
        });
    }

    private static async Task<bool> WaitForFileReadyAsync(string path, CancellationToken cancellationToken)
    {
        long lastLength = -1;
        for (var i = 0; i < 5; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var info = new FileInfo(path);
                // 文件长度连续两次不变时再发送，尽量避开发送尚未写完的大文件。
                if (info.Length > 0 && info.Length == lastLength)
                {
                    using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                    return true;
                }

                lastLength = info.Length;
            }
            catch (IOException)
            {
            }

            await Task.Delay(1000, cancellationToken).ConfigureAwait(false);
        }

        return false;
    }

    private static void TrySetTcpKeepAliveOptions(Socket socket)
    {
        try
        {
            socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, 30);
            socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, 10);
            socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, 5);
        }
        catch (SocketException)
        {
        }
        catch (PlatformNotSupportedException)
        {
        }
    }

    private void PublishTransfer(Guid id, string fileName, string target, long bytesTransferred, long totalBytes, TransferStage stage, string message)
    {
        _transferUpdated(new TransferSnapshot
        {
            Id = id,
            LinkName = _config.LinkName,
            Direction = TransferDirection.Send,
            FileName = fileName,
            Target = target,
            BytesTransferred = bytesTransferred,
            TotalBytes = totalBytes,
            Stage = stage,
            Message = message,
            Timestamp = DateTime.Now
        });
    }
}

internal sealed class ReceiverServer : IDisposable
{
    private const int SocketBufferSize = 4 * 1024 * 1024;
    // 接收端按批次刷盘，避免每块都 flush 导致吞吐明显下降。
    private const int FlushChunkInterval = 8;
    private readonly string _ip;
    private readonly int _port;
    private readonly Action<string> _log;
    private readonly Action<TransferSnapshot> _transferUpdated;
    private readonly Dictionary<string, ReceiveLinkConfig> _links;
    private readonly CancellationTokenSource _cts = new();
    private TcpListener? _listener;
    private Task? _listenTask;

    public ReceiverServer(string ip, int port, IReadOnlyCollection<ReceiveLinkConfig> links, Action<string> log, Action<TransferSnapshot> transferUpdated)
    {
        _ip = string.IsNullOrWhiteSpace(ip) ? "0.0.0.0" : ip;
        _port = port;
        _log = log;
        _transferUpdated = transferUpdated;
        _links = links.ToDictionary(x => x.LinkIdentity, StringComparer.OrdinalIgnoreCase);
    }

    public void Start()
    {
        var address = _ip == "0.0.0.0" ? IPAddress.Any : IPAddress.Parse(_ip);
        _listener = new TcpListener(address, _port);
        _listener.Start();
        _listenTask = Task.Run(ListenLoopAsync);
        _log($"接收端开始监听: {_ip}:{_port}");
    }

    public void Dispose()
    {
        _cts.Cancel();
        _listener?.Stop();
        if (_listenTask is not null)
        {
            try
            {
                _listenTask.Wait(TimeSpan.FromSeconds(2));
            }
            catch
            {
            }
        }
    }

    // 监听器只负责接入客户端，具体文件接收交给独立任务处理。
    private async Task ListenLoopAsync()
    {
        while (!_cts.IsCancellationRequested && _listener is not null)
        {
            try
            {
                var client = await _listener.AcceptTcpClientAsync(_cts.Token).ConfigureAwait(false);
                _ = Task.Run(() => HandleClientAsync(client, _cts.Token));
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                _log($"接收监听异常: {ex.Message}");
            }
        }
    }

    // 每个客户端连接只负责一个文件事务，这样断线续传和状态归档都更清晰。
    private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
    {
        ConfigureAcceptedClient(client);
        using (client)
        await using (var stream = client.GetStream())
        {
            FileStream? fileStream = null;
            string? tempPath = null;
            string? finalPath = null;
            string linkName = string.Empty;
            Guid transferId = Guid.NewGuid();
            try
            {
                var (_, request) = await TransferProtocol.ReadMessageAsync<StartFileRequest>(stream, cancellationToken).ConfigureAwait(false);
                if (!_links.TryGetValue(request.LinkIdentity, out var link))
                {
                    await TransferProtocol.WriteMessageAsync(stream, MessageTypes.Error, new ErrorResponse
                    {
                        Message = $"未找到链路标识: {request.LinkIdentity}"
                    }, cancellationToken).ConfigureAwait(false);
                    return;
                }

                linkName = link.LinkName;
                PublishTransfer(transferId, linkName, request.FileName, client.Client.RemoteEndPoint?.ToString() ?? "远端", 0, request.FileSize, TransferStage.Queued, "等待接收");
                Directory.CreateDirectory(link.ReceivePath);
                finalPath = Path.Combine(link.ReceivePath, request.FileName);
                tempPath = finalPath + ".part";

                long offset = 0;
                if (File.Exists(finalPath))
                {
                    var existing = new FileInfo(finalPath);
                    if (existing.Length == request.FileSize)
                    {
                        await TransferProtocol.WriteMessageAsync(stream, MessageTypes.Resume, new ResumeResponse
                        {
                            Offset = request.FileSize,
                            Message = "文件已存在"
                        }, cancellationToken).ConfigureAwait(false);
                        PublishTransfer(transferId, linkName, request.FileName, client.Client.RemoteEndPoint?.ToString() ?? "远端", request.FileSize, request.FileSize, TransferStage.Succeeded, "文件已存在");
                        _log($"接收链路[{linkName}]文件已存在，跳过: {request.FileName}");
                        return;
                    }

                    File.Move(finalPath, tempPath, true);
                }

                if (File.Exists(tempPath))
                {
                    // 优先复用未完成的 part 文件，从已落盘位置继续接收。
                    offset = new FileInfo(tempPath).Length;
                }

                fileStream = new FileStream(tempPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, request.ChunkSize, FileOptions.SequentialScan);
                fileStream.Seek(offset, SeekOrigin.Begin);

                await TransferProtocol.WriteMessageAsync(stream, MessageTypes.Resume, new ResumeResponse
                {
                    Offset = offset,
                    Message = offset > 0 ? "继续传输" : "开始接收"
                }, cancellationToken).ConfigureAwait(false);
                PublishTransfer(transferId, linkName, request.FileName, client.Client.RemoteEndPoint?.ToString() ?? "远端", offset, request.FileSize, TransferStage.Running, offset > 0 ? "继续接收" : "接收中");

                var chunksSinceLastFlush = 0;
                while (fileStream.Length < request.FileSize)
                {
                    var chunk = await TransferProtocol.ReadChunkAsync(stream, cancellationToken).ConfigureAwait(false);
                    if (chunk.Offset != fileStream.Position)
                    {
                        // 允许发送端按接收端确认的偏移重发，处理断线重连后的补传。
                        fileStream.Seek(chunk.Offset, SeekOrigin.Begin);
                    }

                    await fileStream.WriteAsync(chunk.Buffer.AsMemory(0, chunk.Length), cancellationToken).ConfigureAwait(false);
                    chunksSinceLastFlush++;
                    // 每块都刷盘会显著拖慢大文件传输，改为按批次刷盘并保留断点文件。
                    if (chunksSinceLastFlush >= FlushChunkInterval)
                    {
                        await fileStream.FlushAsync(cancellationToken).ConfigureAwait(false);
                        chunksSinceLastFlush = 0;
                    }

                    await TransferProtocol.WriteMessageAsync(stream, MessageTypes.Ack, new AckResponse
                    {
                        NextOffset = fileStream.Position
                    }, cancellationToken).ConfigureAwait(false);
                    PublishTransfer(transferId, linkName, request.FileName, client.Client.RemoteEndPoint?.ToString() ?? "远端", fileStream.Position, request.FileSize, TransferStage.Running, "接收中");
                }

                var (completeType, _) = await TransferProtocol.ReadMessageAsync<CompleteRequest>(stream, cancellationToken).ConfigureAwait(false);
                if (!string.Equals(completeType, MessageTypes.Complete, StringComparison.Ordinal))
                {
                    throw new IOException("缺少完成消息");
                }

                await fileStream.FlushAsync(cancellationToken).ConfigureAwait(false);
                fileStream.Dispose();
                fileStream = null;
                File.Move(tempPath, finalPath, true);
                PublishTransfer(transferId, linkName, request.FileName, client.Client.RemoteEndPoint?.ToString() ?? "远端", request.FileSize, request.FileSize, TransferStage.Succeeded, "接收成功");
                _log($"接收链路[{linkName}]成功接收文件: {request.FileName}");
            }
            catch (Exception ex)
            {
                PublishTransfer(transferId, linkName, Path.GetFileName(finalPath ?? tempPath ?? "未知文件"), client.Client.RemoteEndPoint?.ToString() ?? "远端", 0, 0, TransferStage.Failed, ex.Message);
                _log($"接收链路[{linkName}]接收失败: {ex.Message}");
            }
            finally
            {
                fileStream?.Dispose();
            }
        }
    }

    private static void ConfigureAcceptedClient(TcpClient client)
    {
        client.ReceiveBufferSize = SocketBufferSize;
        client.SendBufferSize = SocketBufferSize;
        client.NoDelay = true;
        client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

        try
        {
            client.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, 30);
            client.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, 10);
            client.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, 5);
        }
        catch (SocketException)
        {
        }
        catch (PlatformNotSupportedException)
        {
        }
    }

    private void PublishTransfer(Guid id, string linkName, string fileName, string target, long bytesTransferred, long totalBytes, TransferStage stage, string message)
    {
        _transferUpdated(new TransferSnapshot
        {
            Id = id,
            LinkName = linkName,
            Direction = TransferDirection.Receive,
            FileName = fileName,
            Target = target,
            BytesTransferred = bytesTransferred,
            TotalBytes = totalBytes,
            Stage = stage,
            Message = message,
            Timestamp = DateTime.Now
        });
    }
}
