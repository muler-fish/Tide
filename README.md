# 潮汐 · Tide

X 岛匿名版的非官方 Windows 客户端。基于 **WPF / .NET 10 / WPF UI**，采用紧凑的三栏布局，支持浏览、读串、订阅与发言。

[下载正式版](https://github.com/muler-fish/Tide/releases/latest) · [更新记录](CHANGELOG.md) · [文档](docs/README.md) · [反馈问题](https://github.com/muler-fish/Tide/issues)

## 下载与运行

当前版本：**0.2.0**。运行环境：**Windows 10/11 x64**。

1. 在 [Releases](https://github.com/muler-fish/Tide/releases/latest) 下载 `Tide-0.2.0-win-x64.zip`。
2. 解压到任意目录，运行 `Tide.exe`。自包含版本，无需另装 .NET。
3. 更新时先退出旧版，再使用新版程序；默认资料目录相同，可继续使用原有设置。

程序及随附许可约 **79.64 MB**；加上启动时提取的运行时文件，估算约 **87.86 MB**。个人资料、缓存和下载图片另计。具体实测与估算边界见[交付验证](docs/verification.md)。

## 功能

- **三栏阅读**：板块导航、串列表、串阅读区；支持拖动栏宽并记住窗口布局。
- **连续浏览**：列表自动续页、串内双向续页、页码跳转、阅读位置保存、只看 PO。
- **通用订阅**：多个订阅号、备注、切换、明文编辑；兼容中文订阅号，使用快照和短期缓存减少重复加载。
- **正文与链接**：彩色饼干、PO / SAGE 标记、站内串预览、板块跳转确认、引用小窗、官方公告和 tips。
- **图片查看**：独立窗口、缩放、拖动、旋转、保存原图；看到末图时尝试加载下一页。
- **发串与回应**：可选标题和名称、附图、草稿、发送前预览、颜文字及最近使用、防剧透、骰子。
- **本机管理**：历史、板块收藏、屏蔽串、右键复制与回应。
- **阅读外观**：浅色 / 深色、统一 MiSans、可调字号、可选惯性滚动。

第 13、14 项点击状态与字体调整已交付，仍保留在 [TODO](TODO.txt) 等待视觉验收；已验收项目见[归档](docs/todo-archive.md)。

## 饼干与本机数据

普通浏览无需导入饼干；发言和受限内容需要有效饼干。左下角 **饼干管理** 支持二维码图片、剪贴板图片、完整 userhash、Cookie 头及 JSON 导入。

- 默认资料目录：`%LOCALAPPDATA%\TideIsland`。
- 饼干和订阅号通过当前 Windows 用户的 DPAPI 加密保存；订阅号可在管理窗口主动查看、复制。
- 设置、历史、草稿、屏蔽和正文缓存保存在本机；串订阅保存在 X 岛服务端。
- 需要独立资料目录时：`Tide.exe --data-dir "D:\MyTideData"`。
- 同一资料目录只能运行一个实例。

## 快捷键

| 快捷键 | 操作 |
| --- | --- |
| Ctrl+L | 跳串输入框 |
| F5 | 刷新串列表 |
| Ctrl+R | 刷新正文 |
| Ctrl+D | 订阅 / 取消订阅 |
| Enter | 执行页码或串号跳转 |
| Esc | 关闭子窗口 |
| 看图时滚轮 / + / − | 缩放 |
| 看图时 ← / → | 上一张 / 下一张 |
| 看图时 1 / 0 / R | 原始比例 / 适应窗口 / 向左旋转 |

## 从源码构建

需要 **Windows 和 .NET 10 SDK**，在项目根目录执行：

```powershell
dotnet restore Tide.slnx
dotnet build Tide.slnx -c Release --no-restore
dotnet run --project src/Tide.Desktop
```

构建自包含发布包：

```powershell
.\tools\Build.ps1 -SelfContained
```

此脚本会执行核心测试并将程序输出到 `release/<版本>/`。日常修改按影响范围选择专项验证，命令及目录说明见[开发指南](docs/development.md)。

## 当前边界

- GIF 在客户端显示首帧，保存时保留原文件；暂不支持视频、WebP 发图、剪贴板贴图。
- 暂无新回复推送、阅读进度云同步、举报或全岛搜索。
- 接口依赖 X 岛服务；暂未实现动态域名发现和多站点切换。
- 暂无代码签名和自动更新；真实发言仍以使用时的服务器结果为准。

## 来源与许可

交互参考 [摸鱼岛](https://github.com/junbluedd/Moyu-Island)，接口约定参考 [雾岛](https://github.com/123Duo3/XDNMB)和[霞岛](https://github.com/orzogc/xdnmb)。本项目不是 X 岛官方软件。

项目自身的源码暂未指定开源许可证。第三方组件、字体与颜文字资料分别遵循其原有许可和来源说明，见 [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md)；发布包附带相关许可文件。
