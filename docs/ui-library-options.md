# 潮汐 · WPF UI 组件选型

资料核对日期：2026-10-20 至 2026-10-21。本文记录选型时的比较；后续用户已选择 WPF + WPF UI，4.3.0 的实际集成和验证见 [0.2.0 试用版](wpf-ui-preview-0.2.0.md)。其他候选未做集成原型。

选型开始时桌面项目使用 `net10.0-windows` 和 WPF，没有第三方 UI 包；已有业务层与三栏阅读结构可继续沿用。当前试用代码已加入 WPF UI。依据：[项目文件](../src/Tide.Desktop/Tide.Desktop.csproj)、[原有设计](design.md)。

范围说明（2026-10-21 补充）：上轮为了减少返工，默认沿用 WPF，并偏向 Fluent 风格；这两个前提并非用户已经确认的限制。本文只覆盖这条路径，没有逐一评估全部桌面框架，也没有证明其他方案不合适。更多 WPF 库和跨框架路线见[候选补充与暂缓理由](ui-framework-alternatives.md)。

在沿用 WPF、偏向 Fluent 的前提下，建议把 **WPF UI 作为优先试用候选**，用一个真实读串窗口的小型原型（PoC）验证后再决定。这个排序是针对本项目的判断：需要现成标题栏、输入、菜单、对话框和主题，减少自定义控件模板维护，同时重新设计阅读密度。

## 三个主要候选

| 候选 | 本项目中的定位 | 需要验证的重点 |
| --- | --- | --- |
| WPF UI | 优先试用，负责窗口外壳和通用控件 | 标题栏交互、现有资源冲突、紧凑尺寸、正文与虚拟化保持正常 |
| ModernWpf | Fluent / WinUI 风格的备选，可对照实际原型评估 | 当前 1.x 的预发布状态、具体可用包、资源和接口变化 |
| .NET 10 内置 Fluent | 不增加第三方 UI 包的对照方案 | 能否以较少自定义模板达到目标；标题栏、对话框等需要多少额外工作 |

以上为选型判断；不能仅凭组件 Gallery 的截图决定，也未确认用户所指旧 A 岛客户端的实现技术。

### WPF UI

- 定位是为 WPF 提供 Fluent 风格，包含基础控件样式及 `NavigationView`、`NumberBox`、对话框、`Snackbar` 等扩展控件。官方示例使用 `FluentWindow` 和 `TitleBar`。来源：[官方仓库](https://github.com/lepoco/wpfui)。
- `TitleBar` 提供最小化、最大化、关闭相关能力，以及左、中、右内容区域，适合评估把跳串与常用操作并入标题栏。是否适合潮汐的布局仍属待验证判断。来源：[TitleBar API](https://wpfui.lepo.co/api/Wpf.Ui.Controls.TitleBar.html)。
- 主题支持浅色、深色、高对比度，并有系统主题及强调色跟随机制；动态主题要求颜色和画刷使用动态资源。来源：[官方主题文档](https://wpfui.lepo.co/documentation/themes.html)。
- 已确认 `4.3.0` 发布标签的项目目标包含 `net10.0-windows`，且有对应 NuGet 页面；这支持进入试用，不代表已经与本项目兼容。来源：[4.3.0 项目文件](https://github.com/lepoco/wpfui/blob/4.3.0/src/Wpf.Ui/Wpf.Ui.csproj)、[4.3.0 包页面](https://www.nuget.org/packages/WPF-UI/4.3.0)。
- 许可文件为 MIT，正文要求保留版权及许可声明。来源：[官方 LICENSE](https://github.com/lepoco/wpfui/blob/main/LICENSE)。

### ModernWpf

- 当前官方定位是基于 WPF 的 Fluent 样式及 WinUI 风格控件；包含浅色、深色、高对比度、紧凑资源，以及 `NavigationView`、`ContentDialog`、`NumberBox`、`InfoBar` 等。来源：[官方 README](https://github.com/Kinnara/ModernWpf)。
- 当前 1.x 文档有 `TitleBar` 内容区域和窗口材质支持；材质存在系统版本与窗口条件限制，并提供纯色回退。来源：[1.0.0-preview.4 说明](https://github.com/Kinnara/ModernWpf/blob/master/docs/release-notes-1.0.0-preview.4.md)。
- 官方已明确列出 `net10.0-windows7.0`，并说明此目标的基础控件使用 WPF 官方 `PresentationFramework.Fluent`；不能按旧资料判断它仅支持早期 .NET。来源：[支持目标与资源入口](https://github.com/Kinnara/ModernWpf#supported-targets)。
- 版本需特别区分：README 将 1.x 标为活跃预发布线，0.9.x 已冻结、不再维护，且不承诺与 0.9.x 的源码或二进制兼容；当前文档包含 `1.0.0-rc.1`。尚未在本项目确认可还原包、接口及运行表现。来源：[项目状态](https://github.com/Kinnara/ModernWpf#project-status)、[RC 说明](https://github.com/Kinnara/ModernWpf/blob/master/docs/release-notes-1.0.0-rc.1.md)。
- 许可文件为 MIT，正文要求保留版权及许可声明。来源：[官方 LICENSE](https://github.com/Kinnara/ModernWpf/blob/master/LICENSE)。

### .NET 10 内置 Fluent

- WPF 自 .NET 9 起内置面向 Windows 11 观感的 Fluent 主题，提供浅色、深色和系统强调色，可通过 XAML `ThemeMode` 或主题资源字典启用；本项目已使用 .NET 10。来源：[微软 .NET 9 说明](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/whats-new/net90)、[本地项目文件](../src/Tide.Desktop/Tide.Desktop.csproj)。
- .NET 10 继续补齐 `TextBox`、`RichTextBox`、`GridSplitter` 等控件样式并修复高对比度等问题；微软仍说明 Fluent 样式支持在持续完善。来源：[微软 .NET 10 说明](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/whats-new/net100)。
- 它可以作为基础主题对照。标题栏与工具栏一体化仍需单独设计窗口外壳；WPF 的 `WindowChrome` 能扩展非客户区并保留窗口行为，但要求应用提供相应模板。来源：[WindowChrome 文档](https://learn.microsoft.com/en-us/dotnet/api/system.windows.shell.windowchrome?view=windowsdesktop-10.0)。据此判断，这条路线的新增依赖少，窗口模板维护工作可能更多。
- `dotnet/wpf` 仓库许可为 MIT。来源：[官方 LICENSE.TXT](https://github.com/dotnet/wpf/blob/main/LICENSE.TXT)。

### 暂不列首选：iNKORE.UI.WPF.Modern

官方定位是 WPF 的 Fluent 2 样式和控件。当前 `LICENSE.md` 是自定义条款：包含或引用它的软件须完整公开源码，另有协议许可的情况除外；用于商业产品需 iNKORE Studios 明确书面许可。不能把它写成 MIT。本轮只记录许可原文事实，因此暂不列为优先试用方案。来源：[官方仓库](https://github.com/iNKORE-NET/UI.WPF.Modern)、[官方 LICENSE.md](https://github.com/iNKORE-NET/UI.WPF.Modern/blob/main/LICENSE.md)。

## 阅读界面的调整方向

以下是本轮设计意见，不是组件库自动提供的效果：

- 减少大留白、大品牌欢迎语和大面积绿色，做成紧凑的桌面阅读器：内容先出现，品牌仅占标题栏的小区域。
- 回复采用连续阅读流与薄分隔线，减少每条回复独立的大圆角卡片。
- 顶部合并标题栏与常用工具区，同时保留足够拖动区域和标准窗口操作。
- 强调色主要用于发言人饼干、选中项及主要操作；正文使用中性色，普通饼干、红名和 SAGE 保留各自的颜色区分。
- 串列表可试用右侧小缩略图与文字并排，提高同屏串数量；具体尺寸由真实内容对照决定。
- Gallery 只用来选控件与核对交互；读串区域继续按论坛阅读设计，不照搬整页设置卡片布局。

引入组件的另一个实际价值是减少模板维护：本项目曾因 `TextBox.Padding` 与内容宿主 `Margin` 重复计算而裁字。现成控件仍需回归检查，但有助于减少自行重写基础模板的范围。依据：[0.1.3 排版修复记录](text-layout-0.1.3.md)。

## 迁移边界与原型验收

若选择本篇的 WPF 改造路线，则保留 WPF、`Tide.Core` 业务逻辑及“版块 / 串列表 / 正文”三栏结构。先替换窗口外壳和通用控件，再调整列表与回复模板。正文解析、引用、图片、饼干、SAGE、字号设置和现有发言流程应继续工作；安全行为见[现有设计](design.md)。

PoC 应至少使用真实主窗口结构及固定串内容，验证：

1. 固定候选包版本后，在本项目 `net10.0-windows` 配置下完成还原、构建和启动，检查资源覆盖与警告。
2. 标题栏拖动、缩放、双击最大化、最小化及工具按钮命中正常；系统不支持的材质能回退。
3. 中文输入法、长输入、Tab 焦点和菜单操作正常；不同缩放下不裁字。
4. 浅色、深色及高对比度下，正文、饼干、红名、SAGE 和选中态清楚可辨。
5. 现有字号、颜文字换行、图片、引用、列表虚拟化和长串滚动无回退；同一窗口尺寸下比较内容密度。

本选型阶段仅新增记录。后续集成已经完成基础运行和视觉对照，未实测项在 [0.2.0 试用版文档](wpf-ui-preview-0.2.0.md) 中单列，不将选型清单等同于全部验收通过。
