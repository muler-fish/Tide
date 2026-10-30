# 潮汐 UI 技术路线补充比较

核对日期：2026-10-21。本文是一手资料比较，不包含跨框架原型或性能测试。后续用户已选择 WPF + WPF UI，并完成 [0.2.0 试用版集成](wpf-ui-preview-0.2.0.md)；其他路线仍为资料候选。

上轮[WPF 组件选型](ui-library-options.md)实际上限定了“保留 WPF”的范围，未把这个前提讲清楚，也漏列了部分 WPF 库。它不能解释为已经全面评测并排除了 Avalonia、WinUI 3、Flutter、Web 桌面和 Qt 路线。

本文把官方可核实的技术事实与针对潮汐的判断分开。“当前暂缓”表示本轮不优先迁移，不表示框架能力不足或永久淘汰。

## 当前推荐

结合目前明确的“日常可用的 Windows X 岛客户端”目标，推荐 **WPF + WPF UI，读串区域单独设计为紧凑的阅读布局**。用户已确认试用；实现、验证与尚未实测范围见 [试用版文档](wpf-ui-preview-0.2.0.md)。

理由：WPF UI 提供本项目需要的标题栏、主题和通用控件，已核到 .NET 10 发布目标；同时可继续使用现有正文渲染、图片、引用和窗口适配代码，把改动集中在界面组织与交互上。[官方仓库](https://github.com/lepoco/wpfui)、[4.3.0 项目文件](https://github.com/lepoco/wpfui/blob/4.3.0/src/Wpf.Ui/Wpf.Ui.csproj)

视觉方向为统一标题栏与工具栏、紧凑串列表、连续回复与细分隔线、克制的强调色，并保留饼干、红名与 SAGE 区分。组件库负责基础控件一致性，具体阅读体验通过真实内容验证。

现阶段没有明确跨平台需求，也没有证据表明现有 WPF 无法实现上述布局，因此迁移到另一框架的收益不足以优先于完善阅读功能。若后续明确增加其他平台，或原型暴露 WPF 难以满足的具体需求，再重评 Avalonia、WinUI 3 或 Web UI 路线。当前不以未测量的性能差异作为推荐依据。

## 路线与组件的层级

| 路线 | UI 与运行基础 | 本项目的变化范围 | 当前判断 |
| --- | --- | --- | --- |
| WPF + 主题/控件库 | 现有 C#、WPF XAML | 替换资源、模板和窗口外壳 | 保留候选；补齐库比较 |
| Avalonia + SukiUI / Semi / FluentAvalonia | C#、Avalonia XAML | 迁移呈现层及 WPF 耦合代码 | 保留为主要跨框架候选 |
| WinUI 3 | C# 或 C++、WinUI XAML、Windows App SDK | 迁移呈现层与窗口/部署集成 | 保留为 Windows 专用候选 |
| Flutter + Material 3 / Forui / shadcn_flutter / fluent_ui | Dart、Flutter widgets | 新 UI；核心逻辑移植或桥接 | 当前暂缓；跨手机需求明确时提升优先级 |
| Tauri + React + shadcn/ui / Ant Design | Web UI、系统 WebView、Rust 宿主 | 新 Web UI；定义本地业务通信边界 | 保留为 Web UI 对照候选 |
| Electron + Web UI | HTML/CSS/JS、Chromium、Node.js | 新 Web UI；重设宿主与业务边界 | 当前暂缓；可与 Tauri 用同一前端比较 |
| Qt Quick/QML + FluentWinUI3 | QML、Qt Quick，可接 C++/Python | 新 UI 与业务适配 | 当前暂缓 |
| PySide6 + PyQt-Fluent-Widgets | Python、Qt Widgets 绑定及控件库 | 新 UI 与业务适配 | 当前暂缓 |

表格是项目判断，技术依据见以下各节。组件库不能跨上述框架直接互换；同样叫 Fluent 也不表示使用相同的控件实现。

## 保留 WPF 时遗漏的候选

| 方案 | 官方资料确认的能力 | 对潮汐的判断与理由 |
| --- | --- | --- |
| WPF UI | Fluent 风格、窗口标题栏、主题和控件，已核到 .NET 10 发布目标。[来源](https://github.com/lepoco/wpfui) | 仍是低迁移成本的试用候选；这个排序以保留 WPF、偏好 Fluent 为前提。详细记录见[原选型](ui-library-options.md)。 |
| MaterialDesignInXamlToolkit | WPF 的 Material 主题与控件，提供 Material 2/3 资源入口。[来源](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit) | 上轮漏列，本轮保留。若更喜欢 Material 方向，可先在现有 WPF 中比较，无须为这种外观直接迁移到 Flutter。不能据默认示例推断只能做大卡片。 |
| MahApps.Metro | 为现有 WPF 控件提供样式；包含 MetroWindow、主题、对话框与导航。标题栏可定制。[仓库](https://github.com/MahApps/MahApps.Metro)、[窗口文档](https://mahapps.github.io/mahapps.com/docs/controls/metrowindow) | 上轮未展开，本轮保留。对自定义标题栏同样有价值；不能仅因名称含 Metro 就判定过时。具体发布版本及本项目兼容性仍须验证。 |
| HandyControl | WPF 原生控件样式和扩展控件，包含分页、图片浏览等。[官方文档](https://handyorg.github.io/handycontrol/)、[仓库](https://github.com/HandyOrg/HandyControl) | 上轮漏列，本轮保留。它能提供通用控件，是否适合潮汐应比较主题一致性和阅读交互；目前没有硬性淘汰依据。 |
| iNKORE.UI.WPF.Modern | WPF 的 Fluent 2 风格组件；当前使用自定义许可。[仓库](https://github.com/iNKORE-NET/UI.WPF.Modern)、[许可](https://github.com/iNKORE-NET/UI.WPF.Modern/blob/main/LICENSE.md) | 条件性暂缓：许可要求引用它的应用完整开源，另有协议除外；用于商业产品需书面许可。用户尚未确定发行方式，因此先不作默认依赖。这与视觉质量无关，选择符合条款的发行方式后可重新考虑。 |

这些是资料筛选，并未安装上述新增候选或完成兼容性测试。ModernWpf 与 .NET 10 内置 Fluent 继续作为备选，细节保留在[原选型](ui-library-options.md)。

## Avalonia：保留 C#，迁移 UI

**事实：**Avalonia 支持 Windows、macOS、Linux、iOS、Android、WebAssembly，具体支持等级依平台与版本而异；不能把“支持”理解为每个平台保证相同能力。[官方平台说明](https://docs.avaloniaui.net/docs/supported-platforms)

**事实：**其 WPF 迁移文档明确指出样式、数据模板、属性系统和事件命名存在差异。因此可利用 C#/MVVM 经验，但现有 WPF XAML 不是直接替换一个包即可复用。[官方迁移指南](https://docs.avaloniaui.net/docs/migration/wpf)

| 组件 | 一手资料确认的定位 | 本轮状态 |
| --- | --- | --- |
| SukiUI | Avalonia 主题及扩展控件，含浅/深色、配色、通知及对话框；官方明确聚焦桌面 | 保留候选，不能从 Avalonia 的移动支持推导 SukiUI 也支持移动。[仓库](https://github.com/kikipoulet/SukiUI) |
| Semi.Avalonia | 受 Semi Design 启发的 Avalonia 主题；更多自定义控件另指向 Ursa；部分扩展主题分包 | 保留候选，分别核对所选扩展包与版本。[仓库](https://github.com/irihitech/Semi.Avalonia) |
| FluentAvalonia | 为 Avalonia 提供 Fluent 设计及更多 WinUI 风格控件 | 保留候选；它属于 Avalonia，不是 WinUI 3 运行时。[仓库](https://github.com/amwx/FluentAvalonia) |
| Fluid.Avalonia | 已找到明确作者仓库，定位为叠加在 Avalonia FluentTheme 上的 Fluent 2 风格主题 | 待补核；已确认名称所指，但未比较控件覆盖、版本兼容和实际集成。[仓库](https://github.com/Alpaq92/Fluid.Avalonia) |

**项目判断：**若愿意重做 UI 又希望保留 C#，Avalonia 应进入真实候选名单。SukiUI 与 Semi 可以代表不同视觉方向，FluentAvalonia 可代表偏 Windows 的方向；没有依据认定其中任何一个必然最好看。

## WinUI 3：Windows 专用的另一套 UI

**事实：**WinUI 3 是 Windows App SDK 的组成部分，使用 XAML，支持 C# 和 C++，面向 Windows 桌面应用。它不是 WPF 主题包。[微软介绍](https://learn.microsoft.com/en-us/windows/apps/winui/winui3/)

**项目判断：**如果目标一直是 Windows，并希望采用微软这套桌面控件，可以保留为候选。使用 C# 不代表可沿用 WPF 的 RichTextBox、FlowDocument、窗口实现和控件模板；需要单独验证正文功能及发布流程。

## Flutter：Dart UI，可覆盖桌面与手机

**事实：**Flutter 的应用层采用 Dart/widgets；框架与渲染引擎构成自身 UI 栈，插件承担平台集成。官方支持构建 Windows、macOS、Linux 桌面应用，也提供移动平台集成。[架构](https://docs.flutter.dev/resources/architectural-overview)、[桌面支持](https://docs.flutter.dev/platform-integration/desktop)

| UI 选项 | 一手资料确认的定位 | 本轮状态 |
| --- | --- | --- |
| Material 3 | Flutter 官方 Material 设计组件及主题路线 | 保留在 Flutter 内部候选中。[迁移说明](https://docs.flutter.dev/release/breaking-changes/material-3-migration) |
| Forui | 受 shadcn/ui 启发、可定制的 Flutter UI 库 | 保留在 Flutter 内部候选中。[官网](https://forui.dev/) |
| shadcn_flutter | 面向 Flutter 的 shadcn 风格组件、主题及工具；不是 React shadcn/ui 本体 | 保留在 Flutter 内部候选中。[作者仓库](https://github.com/sunarya-thito/shadcn_flutter) |
| fluent_ui | Flutter 中实现 Fluent/WinUI 风格的组件包 | 保留在 Flutter 内部候选中；不能当作微软 WinUI 3 原生控件。[作者仓库](https://github.com/bdlukaa/fluent_ui) |

**项目判断：**如果手机与桌面共享产品逻辑和设计成为明确目标，Flutter 值得投入原型。当前仅为重做 Windows 阅读界面就转换到 Dart，需要额外解释业务复用、正文选择/引用、桌面窗口和分发成本，故本轮暂缓。

## Tauri / Electron：Web UI 与桌面宿主

**事实：**Tauri 以系统 WebView 渲染 HTML，Rust 宿主与前端通过消息通信；可以选择前端框架。Windows 使用 WebView2。这不是 C# 控件库，也不要求把全部业务都写在前端。[架构](https://v2.tauri.app/concept/architecture/)、[WebView 说明](https://v2.tauri.app/reference/webview-versions/)

**事实：**shadcn/ui 提供可纳入项目修改的组件源码；Ant Design 的 `antd` 是 React 组件实现。两者负责 Web UI，不负责替代桌面宿主。[shadcn/ui](https://ui.shadcn.com/docs)、[Ant Design React](https://ant.design/docs/react/introduce/)

**事实：**Electron 把 Chromium 和 Node.js 包含在应用中，支持用 HTML/CSS/JavaScript 构建 Windows、macOS、Linux 桌面应用。它与 Tauri 的运行基础不同。[Electron 官方介绍](https://www.electronjs.org/docs/latest/)

**项目判断：**如果希望用 Web 布局与组件生态重做阅读器，Tauri + React 应保留为对照候选。`Tide.Core` 不能直接作为 React/Rust UI 库导入；需决定移植逻辑，或保留独立 .NET 进程并定义通信协议。Electron 也可实现此方向，但本轮尚无必要同时做两个宿主原型。

尚未测量这两者在潮汐中的启动、滚动、内存、安装体积和字体表现，不能用“Tauri 必然快”或“Electron 必然卡”代替验证；系统 WebView 与捆绑 Chromium 的区别本身也不是实测结果。

## Qt：区分 QML 与 Widgets

**事实：**Qt Quick 提供 QML API 构建界面，并可通过 C++ 扩展；Qt for Python 的 PySide6 是 Qt 6 官方 Python 绑定，因此 QML 也可由 Python 程序承载。[Qt Quick](https://doc.qt.io/qt-6/qtquick-index.html)、[Qt for Python](https://doc.qt.io/qtforpython-6/)

**事实：**FluentWinUI3 是 Qt Quick Controls 的官方样式，自 Qt 6.8 提供；官方明确说它不是原生样式，并可在支持的平台上运行。部分控件尚不支持该样式，会回退到 Fusion。它不是微软 WinUI 3 框架。[官方样式说明](https://doc.qt.io/qt-6/qtquickcontrols-fluentwinui3.html)

**事实：**PyQt-Fluent-Widgets 属于 Qt/PyQt/PySide 的 Widgets 控件路线，有单独 PySide6 分支及包；它与 Qt Quick/QML 的 FluentWinUI3 样式不是同一个东西。[作者仓库](https://github.com/zhiyiYo/PyQt-Fluent-Widgets)

**项目判断：**两条 Qt 路线都保留为可行方向，本轮暂缓，因为潮汐现有 C# 核心和 WPF UI 均需安排移植或桥接。不能因使用 Python 就断言开发一定更快，也不能因 QML 支持动画就断言论坛阅读效果一定更好。

## 对潮汐的迁移边界

本地事实：`Tide.Core` 的目标为 `net10.0`，HTTP API 和 JSON 存储在此层；它没有 WPF 的项目依赖。它是 C# 路线的主要复用基础。[项目文件](../src/Tide.Core/Tide.Core.csproj)

现有桌面代码仍包含框架绑定：`MainViewModel` 使用 `ICollectionView`/`CollectionViewSource`；`PostBody` 基于 `RichTextBox`/`FlowDocument`；`ImageService` 使用 `BitmapImage`；`WindowPlacement` 结合 WPF 与 Win32。跨框架时不能声称“业务全部无需变动”。[ViewModel](../src/Tide.Desktop/MainViewModel.cs)、[正文](../src/Tide.Desktop/PostBody.cs)、[图片](../src/Tide.Desktop/ImageService.cs)、[窗口](../src/Tide.Desktop/WindowPlacement.cs)

饼干加密目前位于桌面层，使用 Windows DPAPI。更换 Windows UI 框架时可评估复用；若进一步支持其他系统，则需要对应的凭据存储实现。[实现](../src/Tide.Desktop/Infrastructure.cs)

**项目判断：**迁移成本的差异主要来自现有实现能保留多少，以及新增语言、运行时和平台服务边界；这不是对各框架美观、性能或维护质量的绝对排序。

## 接下来如何形成决定

1. 先比较同一份真实内容的三栏布局、字号、信息密度、颜色和交互，再决定是否值得跨框架。
2. 若保留 WPF，补齐 WPF UI、MaterialDesignInXamlToolkit 等视觉方案；若接受迁移，优先比较 Avalonia 与 Tauri 的单窗口样稿，Windows 原生控件优先时加入 WinUI 3。
3. 对进入下一轮的方案固定版本，实际构建并检查中文输入、正文选择、引用、图片、长串滚动、DPI、键盘与窗口行为。
4. 测量启动、内存、滚动和分发体积后再排序；商业使用、扩展包及再分发的许可也应按最终选定依赖核对。

上述顺序是当前项目的建议，不是已经得到测试支持的胜负结论。本轮没有依据评价“AI 对某框架更擅长”，也没有任何框架能保证成品美观。
