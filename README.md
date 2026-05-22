# 文件传输工具

Windows Forms 文件传输工具，支持发送链路、接收链路、传输状态和日志查看。

## 开发环境

- Windows
- .NET SDK 9

## 构建

```powershell
dotnet build FileTransferTool.sln -c Release
```

真实链路配置放在 `Config\linkdata.xml`，该文件默认不提交；仓库只保留 `Config\linkdata.example.xml` 作为模板。

## 发布 WinServer 自包含包

```powershell
dotnet publish FileTransferTool.App\FileTransferTool.App.csproj -c Release -r win-x64 --self-contained true -o artifacts\publish\FileTransferTool.App\win-x64
```

发布包会包含 .NET/WinForms 运行时文件，适合没有安装 .NET Desktop Runtime 的 Windows Server。

## 服务器启动检查

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File tools\Test-ServerCompatibility.ps1 -PackageDir artifacts\publish\FileTransferTool.App\win-x64
```
