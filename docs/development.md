# 开发指南

## 环境与构建

Windows、.NET 10 SDK、PowerShell。WPF UI、ZXing.Net、Emoji.Wpf 版本固定在项目文件中；字体与生成后的颜文字资源随源码提供，正常构建无需 Python。

```powershell
dotnet restore Tide.slnx
dotnet build Tide.slnx -c Release --no-restore
dotnet run --project src/Tide.Desktop -- --data-dir "artifacts/dev-profile"
```

开发时使用独立资料目录，避免与日常客户端同时写入同一份状态。

## 目录

| 目录 | 内容 |
| --- | --- |
| src/Tide.Core | API、数据模型、缓存、订阅、分页和本地存储 |
| src/Tide.Desktop | WPF 窗口、视图模型、渲染、主题、图片及输入交互 |
| tests/Tide.Tests | 核心行为与固定接口测试 |
| tests/Tide.Desktop.Tests | 按功能选择的桌面专项验证 |
| tools | 构建、图标和颜文字资源生成脚本 |
| docs | 设计、实现、调研、验收和第三方声明 |
| release / artifacts | 本地发布及诊断产物，不提交 Git |

## 最小验证

只执行修改部分及直接关联范围的检查。文档、忽略规则等整理不触发整套界面回归。桌面专项使用固定接口和隔离资料，但会操作自己的测试窗口；请选择无需同时操作鼠标键盘的时机。

```powershell
# 0.2.0 点击状态、正文渲染与相关功能
.\tests\Tide.Desktop.Tests\bin\Release\net10.0-windows\Tide.Desktop.Tests.exe --reading-polish artifacts/reading-polish

# 示例：按实际改动选择对应专项
.\tests\Tide.Desktop.Tests\bin\Release\net10.0-windows\Tide.Desktop.Tests.exe --links artifacts/links
.\tests\Tide.Desktop.Tests\bin\Release\net10.0-windows\Tide.Desktop.Tests.exe --images artifacts/images
.\tests\Tide.Desktop.Tests\bin\Release\net10.0-windows\Tide.Desktop.Tests.exe --feedback-motion artifacts/feedback-motion

# 核心链接专项；完整核心测试仅在所需范围确实覆盖全部时执行
dotnet tests/Tide.Tests/bin/Release/net10.0/Tide.Tests.dll --links
```

其他专项包括 `--subscriptions`、`--subscription-cache`、`--thread-navigation`、`--reading-progress`、`--cookie-import`、`--emoticons`、`--emoji`、`--tips`、`--spoilers`、`--post-preview`、`--layout-memory`、`--context-actions`。完整分派见 [Program.cs](../tests/Tide.Desktop.Tests/Program.cs)，各轮实际运行范围见[交付验证](verification.md)。

`--subscription-live` 和应用的 `--smoke-dir` 是显式联网验证入口，不属于上述离线专项；不要用日常饼干或个人订阅号进行自动写入测试。

## 发布

全流程构建脚本会编译、运行核心测试、生成自包含程序、复制许可、输出 SHA-256，并检查发布目录低于 100,000,000 字节：

```powershell
.\tools\Build.ps1 -SelfContained
```

对于已经完成局部验证的版本，可直接发布，避免重复执行无关测试：

```powershell
dotnet publish src/Tide.Desktop/Tide.Desktop.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -p:DebugType=None -p:DebugSymbols=false -o release/0.2.0
```

直接发布后还须按 `Build.ps1` 的同一清单附带 `licenses/`，生成校验值并核对体积。运行时提取目录另占磁盘空间，不能仅用 EXE 大小表示总占用。版本号需同步项目元数据、主窗口和设置窗口。

二进制和压缩包上传 GitHub Releases，不提交到源码历史。旧预览版及本机调试产物保留在本地。

## 颜文字资源

```powershell
python tools/Build-Emoticons.py
```

仅在修改资料或生成规则时运行。使用仓库内的网页快照和蓝岛补充数据，不联网；Gboard 字典已排除。Python 不是编译客户端所需的依赖。
