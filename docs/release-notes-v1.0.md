# v1.0 发布说明

发布日期：2026-05-23

## 发布内容

- 首个正式版本，面向 Windows/Windows Server 的文件传输节点工具。
- 支持发送链路、接收链路和中转节点配置。
- 支持目录监听、定时扫描兜底、文件过滤和发送后备份归档。
- 支持 TCP 分块传输、断点续传、`.part` 安全落盘和完成后改名。
- 提供发送配置、发送状态、接收配置、接收状态、全部总览和运行日志界面。
- 提供 README、产品文档、用户操作说明和软件截图。

## 构建方式

```powershell
dotnet publish FileTransferTool.App\FileTransferTool.App.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -o artifacts\publish\FileTransferTool.App\win-x64
```

## 发布包命名

```text
FileTransferTool-win-x64-v1.0.zip
```

## 验证项

- `dotnet format FileTransferTool.sln --verify-no-changes`
- `dotnet build FileTransferTool.sln -c Release`
