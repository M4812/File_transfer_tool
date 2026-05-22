# 文件传输工具

一个面向 Windows/Windows Server 的文件传输节点工具。程序基于 Windows Forms 和 .NET 9 开发，同一套程序可以同时配置发送链路和接收链路，用于在多台服务器之间按目录、链路标识和端口转发文件。

## 主要能力

- 发送链路：监控本机目录，将符合过滤规则的正式文件发送到远端节点。
- 接收链路：监听指定 IP/端口，按链路标识把文件落盘到对应目录。
- 中转节点：同一台服务器可以一边接收、一边继续发送，适合 `A -> B -> C` 这类链路。
- 断点续传：接收端会优先复用未完成的 `.part` 文件，发送端按接收端确认的偏移继续传输。
- 安全落盘：接收中的文件先写入 `.part`，完成后再改成正式文件名，避免中转时转发半成品。
- 状态查看：界面提供发送配置、发送状态、接收配置、接收状态、全部总览和运行日志。

## 运行环境

- Windows 10/11 或 Windows Server。
- 发布包为 `win-x64` 自包含包，目标服务器通常不需要额外安装 .NET Desktop Runtime。
- 如果在 Windows Server 上运行，需要有 Desktop Experience 和交互式桌面会话。

## 配置文件

程序启动时读取：

```text
Config\linkdata.xml
```

仓库不会提交真实环境配置，只保留模板：

```text
Config\linkdata.example.xml
```

首次部署时，可以复制模板并按实际链路修改：

```powershell
Copy-Item Config\linkdata.example.xml Config\linkdata.xml
```

配置结构示例：

```xml
<链路配置>
  <发送链路>
    <链路信息>
      <链路名称>示例发送链路</链路名称>
      <监视目录>D:\FileTransfer\Send</监视目录>
      <发送地址>127.0.0.1</发送地址>
      <发送端口>12092</发送端口>
      <备份目录>D:\FileTransfer\Backup</备份目录>
      <链路标识>demo</链路标识>
      <过滤类型>*.*</过滤类型>
    </链路信息>
  </发送链路>
  <接收链路>
    <接收端口>12092</接收端口>
    <接收IP>0.0.0.0</接收IP>
    <链路信息>
      <链路名称>示例接收链路</链路名称>
      <远端IP>127.0.0.1</远端IP>
      <接收目录>D:\FileTransfer\Receive</接收目录>
      <链路标识>demo</链路标识>
    </链路信息>
  </接收链路>
</链路配置>
```

发送链路和接收链路通过 `链路标识` 匹配。发送端填写的标识必须能在接收端配置中找到。

## 本地开发

要求安装 .NET SDK 9。

```powershell
dotnet restore FileTransferTool.sln
dotnet build FileTransferTool.sln -c Release
```

从源码运行：

```powershell
dotnet run --project FileTransferTool.App\FileTransferTool.App.csproj -c Release
```

## 发布自包含包

生成 Windows Server 可用的 `win-x64` 自包含发布目录：

```powershell
dotnet publish FileTransferTool.App\FileTransferTool.App.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -o artifacts\publish\FileTransferTool.App\win-x64
```

打包 zip：

```powershell
Compress-Archive `
  -Path artifacts\publish\FileTransferTool.App\win-x64\* `
  -DestinationPath artifacts\release\FileTransferTool-win-x64.zip `
  -Force
```

## 发布包验证

验证发布目录中是否包含 WinForms 自包含运行所需文件，并做一次启动检查：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass `
  -File tests\Verify-WindowsServerPackage.ps1 `
  -PackageDir artifacts\publish\FileTransferTool.App\win-x64
```

也可以使用面向服务器部署的检查脚本：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass `
  -File tools\Test-ServerCompatibility.ps1 `
  -PackageDir artifacts\publish\FileTransferTool.App\win-x64
```

## 部署步骤

1. 从 GitHub Releases 下载 `FileTransferTool-win-x64.zip`。
2. 解压到目标服务器目录，例如 `D:\Apps\FileTransferTool`。
3. 按实际环境修改 `Config\linkdata.xml`。
4. 双击运行 `FileTransferTool.App.exe`。
5. 在界面中确认发送链路、接收链路和运行日志。

## 注意事项

- 不要把生产环境的 `Config\linkdata.xml` 提交到仓库。
- 接收目录、监视目录和备份目录需要提前保证磁盘空间充足。
- 多台服务器互传时，需要确认防火墙允许接收端口入站。
- 中转节点建议让接收目录和下一条发送链路的监视目录保持一致。
