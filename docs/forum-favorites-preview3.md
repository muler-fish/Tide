# 0.2.0-preview.3 · 版块收藏与侧栏交互

日期：2026-10-21 至 2026-10-22。对应截图中的侧栏滚动条、版块星标和刷新按钮反馈。

## 本轮设计

- 刷新保留原有循环箭头，不增加可见文字。32 DIP 方形圆角底座沿用发串按钮的颜色及状态模板；保留“刷新串列表”无障碍名称、提示文字和 F5。
- 版块目录的滚动容器向右延伸 8 DIP，内容右侧预留 18 DIP。预留空间覆盖 WPF UI 滚动条的展开宽度，避免盖住行背景、名称和星标。
- 每行的名称按钮与 32 DIP 星标是并列操作。名称负责导航，星标只修改收藏；星标空间始终保留，显示时不挤动文字。
- 未收藏时星标默认不显示，鼠标悬停或键盘焦点进入该行后显示空心星；收藏后实心黄色星标常显。Tab 可进入星标，空格键和屏幕阅读器 Toggle 均可切换收藏。
- “收藏版块”位于目录顶部，与其余分组共用一个滚动区域。收藏项直接点击即可进入，不必展开原分组；目录分组仍默认收起。
- 长名称用省略号显示，并保留完整名称提示和可访问性名称。浅色主题的星标为 `#A87500`，深色为 `#F2C94C`；实心 / 空心和切换状态与颜色共同表达收藏。

## 实现与数据

- `UserState.FavoriteForums` 保存本地版块快照，独立于串收藏。时间线 `t1` 与普通版块 `f1` 不会因数字 ID 相同而冲突。
- 新增和取消立即保存。目录刷新更新名称、版规等信息，暂时缺少的入口保留快照，因此维护或离线不会清空收藏。
- 旧资料缺少字段时使用空集合；读取时处理 null、无效项和重复项。收藏顺序按添加顺序保留。
- `MainViewModel` 增量同步快捷收藏集合与星标键集合；切换星标不改变当前串、当前版块或发起请求。
- `ForumFavoriteToggleButton.OnToggle` 是鼠标、键盘和自动化共同的切换入口，勾选状态由数据绑定回写。独立的 `ToggleCommand` 避免普通点击重复执行两次，也避免屏幕阅读器只改变外观而没有保存。

关键入口：[MainWindow.xaml](../src/Tide.Desktop/MainWindow.xaml)、[ForumFavoriteToggleButton.cs](../src/Tide.Desktop/ForumFavoriteToggleButton.cs)、[MainViewModel.cs](../src/Tide.Desktop/MainViewModel.cs)、[LocalStore.cs](../src/Tide.Core/LocalStore.cs)。

## 技能与验证

本轮继续使用 `ui-ux-pro-max`，采用其键盘可达、固定操作区、滚动遮挡、浅深色和状态不只靠颜色的要求。针对 WPF 查询了键盘焦点与 ToggleButton 指引；沿用现有 WPF UI 设计系统，不生成新的视觉方向。

离线状态测试：`ForumFavoritesStateChecks`，覆盖旧资料、同 ID 不同类型、增删持久化、重启恢复、目录更新、暂缺入口和断网保留。

真实界面测试：`ForumNavigationChecks`，使用独立资料与固定接口，检查星标同步、键盘操作、滚动条边界、浅深色和刷新状态。源码检查不访问公网。

完整离线回归已通过，报告和截图位于 `artifacts/ui-qa-0.2.0-preview3/`：

- 星标未聚焦时隐藏、聚焦后揭示、收藏后常显；标准 Toggle 自动化与普通点击各切换一次，目录与收藏栏同步。
- 刷新只含循环箭头且有不透明底座；F5 使用同一命令，请求期间禁用且不重复发送请求，完成后恢复。
- 900 / 1060 / 1280 DIP 窗口、148 DIP 最小侧栏中，控件最右边为 117.6 DIP、滚动条左侧为 120.8 DIP，留有 3.2 DIP 间隔。
- 黄色星标在侧栏、选中与悬停背景上的最低图形对比度：浅色 3.32、深色 7.17。
- 收藏旧资料兼容、f/t 标识、重开恢复、目录更新、缺失入口保留及离线取消全部通过。
- 既有标题栏、八类子窗口、30 组尺寸计算、身份标记和文本排版回归全部通过。

浅色与深色实测截图：[浅色](../artifacts/ui-qa-0.2.0-preview3/sidebar-favorites-light.png)、[深色](../artifacts/ui-qa-0.2.0-preview3/sidebar-favorites-dark.png)、[窄侧栏](../artifacts/ui-qa-0.2.0-preview3/sidebar-favorites-narrow.png)。这些截图使用测试目录和内容，不是实时论坛内容。

```powershell
dotnet build Tide.slnx -c Release
.\tests\Tide.Desktop.Tests\bin\Release\net10.0-windows\Tide.Desktop.Tests.exe --forum-navigation artifacts/forum-navigation-preview3
```

最终 EXE 使用隔离资料完成启动、三个子窗口及星标恢复和取消检查，全部通过。取消操作同时核对保存结果和界面更新，截图已确认对应快捷入口移除、其他收藏保留；报告和截图位于 `artifacts/window-published-0.2.0-preview3-final/`，版本、哈希和完整结果见 [verification.md](verification.md)。本轮未重跑完整公开 API 读串验收；实际 EXE 正常启动时仍显示 HTTP 502，收藏操作在接口失败时正常完成。用户此前提供了维护信息，此处不推测维护原因。

## 交付

新版路径为 `release/0.2.0-preview.3/Tide.exe`，界面显示“预览 3”。退出旧版后运行，默认沿用原资料目录；需要独立试用时使用 `--data-dir`。旧版 EXE 保留。
