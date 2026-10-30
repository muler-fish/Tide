# 潮汐 0.1.2 · 界面调整设计与验收

日期：2026-10-20。范围：本轮两张标注截图及补充的主楼图片需求。

## 使用的设计技能

沿用 `ui-ux-pro-max`，技能源文件：
`SKILL.md`。

这是已有 WPF 客户端的定向调整，采用技能的组件问题流程，没有重新生成产品配色或更换布局。执行了 UX 查询 `navigation accordion focus status color`，以及 WPF 查询 `templates binding resources`、`data binding dynamic resources control template`。

实际采用的规则：

- 状态不能只依赖颜色：SAGE 保留文字；红名饼干提供“红名发言人”提示，PO 身份继续使用文字。
- 导航保留可见键盘焦点；分组标题整行可点击，使用 ToggleButton 和双向绑定。
- 同一语义使用同一组件与主题资源；浅色和深色分别检查文字对比度。
- 自定义控件的绑定入口使用 DependencyProperty；用 ControlTemplate 替换分组原生圆圈样式。
- 图片异步加载、限定并发，继续保留虚拟化、取消请求和无图模式；有图列表项预留缩略图高度。

## 需求与结果

| 用户标注 | 实现 |
| --- | --- |
| 删除顶部文案 | 删除两行文案；顶部仅保留跳串和发串操作，高度从 76 调整到 64 DIP |
| 不需要筛选版块 | 删除侧栏输入框及 ForumFilter 状态；中间“在当前列表中查找”保留 |
| 分组符号突兀、默认收起 | 改为细线箭头；关闭时向右、打开时向下；启动时所有分组收起 |
| 发言人饼干突出显示 | 公开 ID 使用青绿文字及浅色底标记；列表、正文、引用弹窗统一 |
| 红名使用红色 | 根据服务端红名标记使用红色文字和浅红底；不把红名直接标成“管理员” |
| 是 SAGE 才提示 | 仅 IsSage 为 true 时显示红底白字 SAGE；否则不占位 |
| 串首图片要加载 | 中间列表新增主楼缩略图；右侧主楼沿用图片控件，并实际验证两处都加载成功 |

## 页面结构与交互

三栏结构继续为“版块导航 → 串列表 → 阅读区”。侧栏的分组收起不改变默认加载综合线的行为；用户点击组名或箭头展开后即可选择版块。折叠状态只在本次窗口生命周期内保留，下次启动默认收起。

列表项按“版块 / 串号 → 标题 → 摘要 → 主楼缩略图 → 饼干、SAGE、回复数、阅读进度”排列。没有图片时图片控件整体折叠；没有摘要时不留下空白文本行。标记行与前面的内容间隔 12 DIP；空间不足时标记行可以换行。

正文保留“PO · 串主 / 回应”的角色文字；红名身份与 PO 身份互不排斥。页面没有因此增加新的操作步骤。

## 饼干与 SAGE 的状态规则

`PostIdentity` 只接收 `Post`，不接触登录饼干或其加密值。公开 ID 来自 `UserHash`，红名来自现有 `IsAdmin` 字段，SAGE 来自 `IsSage`。API 当前解析 `admin=1`、`sage=1`，兼容数字及数字字符串，不依据 ID 内容或正文猜测。

`IsAdmin` 是既有数据字段名称，为兼容已保存的帖子数据保留；显示层只将它用于红名样式，不据此声明发言人拥有管理权限。

| 红名 | SAGE | 显示 |
| --- | --- | --- |
| 否 | 否 | 青绿饼干 |
| 是 | 否 | 红色饼干 |
| 否 | 是 | 青绿饼干 + SAGE |
| 是 | 是 | 红色饼干 + SAGE |

| 语义 | 浅色：文字 / 背景 | 深色：文字 / 背景 | 实测对比度（浅 / 深） |
| --- | --- | --- | --- |
| 普通饼干 | `#13765E` / `#DCEDE5` | `#66C5A7` / `#273F34` | 4.57 / 5.47 |
| 红名饼干 | `#AE413B` / `#FCECE8` | `#F3A299` / `#422C29` | 5.06 / 6.43 |
| SAGE | `#FFFFFF` / `#C32635` | 同浅色 | 5.75 / 5.75 |

饼干字号为 12 DIP，SAGE 为 11 DIP；标记使用 4 DIP 圆角和横向 6、纵向 2 DIP 内边距。所有颜色使用语义资源，不在不同页面分别硬编码。

## 主楼图片规则

- 列表和正文共用 `PostImage`，请求 `Post.ImageUrl(true)` 返回的官方图片站缩略图。
- 列表的图片区预留 140 DIP 高度，图片最大宽度为 250 DIP；正文保持自然比例，最大高 260、宽 440 DIP。均使用 Uniform，不裁切图片。
- 缩略图加载后可以点击查看原图；加载失败保留重试按钮。
- 开启无图模式后显示“显示这张图片”，不自动请求缩略图；用户仍能手动打开。
- 图片通过独立、不附带登录饼干的 HttpClient 下载；4 个并发槽、20 MB 下载上限和 80 项内存缓存沿用现有实现。
- 虚拟化容器卸载或换绑时取消旧请求；无图帖子同时清除旧内容，避免残留图片。图片只在内存缓存，离线正文缓存不包含图片文件。

## 源码入口

| 文件 | 职责 |
| --- | --- |
| [MainWindow.xaml](../src/Tide.Desktop/MainWindow.xaml) | 去除文案及版块筛选；列表图片、标记行与阅读模板 |
| [App.xaml](../src/Tide.Desktop/App.xaml) | 分组细箭头模板、键盘焦点与 SAGE 主题资源 |
| [PostIdentity.xaml](../src/Tide.Desktop/PostIdentity.xaml) / [代码](../src/Tide.Desktop/PostIdentity.xaml.cs) | 共用饼干与 SAGE 组件 |
| [MainViewModel.cs](../src/Tide.Desktop/MainViewModel.cs) | 删除版块筛选状态；回复数和 PO 角色显示 |
| [ImageService.cs](../src/Tide.Desktop/ImageService.cs) | 可配置缩略图尺寸、无图 / 失败 / 换绑处理 |
| [Dialogs.cs](../src/Tide.Desktop/Dialogs.cs) | 引用弹窗复用饼干组件 |
| [UiRefinementChecks.cs](../tests/Tide.Desktop.Tests/UiRefinementChecks.cs) | 固定状态组合、主题、换绑、分组交互和紧凑窗口检查 |
| [SmokeRunner.cs](../src/Tide.Desktop/SmokeRunner.cs) | 真实只读 API、主楼图片与无图模式验证 |

新增与调整的非直观逻辑保留中文注释。

## 验收与复现

- Release 构建：无警告、无错误；既有模拟 HTTP 行为测试 17 / 17 通过。
- WPF 回归：分组默认收起、展开和再次收起、标题键盘焦点通过。
- 浅色及深色：红名 × SAGE 四种组合换绑检查，文字颜色、底色、SAGE 可见性及文字对比度通过。
- 引用弹窗复用检查通过；1060 × 720 DIP 紧凑窗口截图检查，标记未横向溢出。
- 原有窗口回归继续通过：当前 1920 × 1080 / 125% 缩放的启动、最大化、还原和两种弹窗；30 组工作区 / DPI **计算**场景通过，没有修改系统显示设置。
- 在线读取参考串 No.69574374；在收藏列表与阅读页检查实际解码后的 BitmapSource，两处主楼图片均已加载；无图模式检查通过。
- 同时复查公开版块、时间线、分页、只看 PO、收藏、历史、引用、离线缓存、过期请求隔离与 DPAPI。没有提交真实发言。

界面检查：

```powershell
dotnet build Tide.slnx -c Release
dotnet tests/Tide.Tests/bin/Release/net10.0/Tide.Tests.dll
.\tests\Tide.Desktop.Tests\bin\Release\net10.0-windows\Tide.Desktop.Tests.exe artifacts/ui-qa-0.1.2
```

最终发布程序的只读验证：

```powershell
.\release\standalone\Tide.exe --smoke-dir .\artifacts\release-qa-0.1.2
.\tests\Tide.Desktop.Tests\bin\Release\net10.0-windows\Tide.Desktop.Tests.exe --exe release/standalone/Tide.exe artifacts/window-published-0.1.2
```

状态组合报告与截图在 `artifacts/ui-qa-0.1.2/`，构建程序的在线预检在 `artifacts/ui-live-0.1.2/`。最终 EXE 的验证记录与校验值见 [verification.md](verification.md)。

本轮没有实测屏幕阅读器朗读，也没有在多台物理显示器或另一台干净 Windows 机器上测试。真实发言链路不在这次界面修改的验收范围。
