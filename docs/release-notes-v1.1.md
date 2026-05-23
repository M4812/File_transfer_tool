# v1.1 发布说明

发布日期：2026-05-23

## 更新内容

- 修复发送配置页和接收配置页在高 DPI / 缩放环境下输入框文字被标签遮挡的问题。
- 修复接收配置页标题和说明区域可能被分组控件遮挡的问题。
- 调整配置页输入框高度、标签宽度、列间距和按钮行距，提升中文输入显示稳定性。
- 修复“浏览”按钮文字显示不完整的问题，统一按钮宽度、内边距和文字居中。
- 发布包内 `Config\linkdata.xml` 使用指定的发布配置文件。

## 发布包

```text
FileTransferTool-win-x64-v1.1.zip
```

## 验证项目

- `dotnet format FileTransferTool.sln --verify-no-changes --verbosity minimal`
- `dotnet build FileTransferTool.sln -c Release`
- `dotnet publish FileTransferTool.App\FileTransferTool.App.csproj -c Release -r win-x64 --self-contained true`
- 发布目录启动检查
- zip 解压后启动检查
- 发送/接收配置页 UI 自动化检查
