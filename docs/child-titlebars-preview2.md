# 0.2.0-preview.2 · 子窗口标题栏统一

日期：2026-10-21。对应用户反馈：发串、饼干管理、阅读设置的顶部仍为蓝色系统标题栏。

## 原因与范围

preview.1 只给主窗口接入了 FluentWindow / TitleBar；这三个窗口仍继承原有 DialogWindow → Window。仅套用按钮、输入框和颜色资源不会替换系统标题栏。原图窗口也直接继承 Window，属于同一遗漏范围。

本次加入公共 `ChildWindow : FluentWindow`，由它创建 WPF UI 标题栏、客户内容区域并处理主题、首次工作区适配和 Esc 关闭。DialogWindow 与 ImageWindow 均复用此外壳。

| 窗口 | 结果 |
| --- | --- |
| 发一个串、参与回应 | 新标题栏；编辑器 Closing 继续保存草稿、保留取消关闭的入口 |
| 饼干管理、阅读设置 | 新标题栏；浅深色同步，原操作流程保留 |
| 草稿箱 | 通过 DialogWindow 自动统一 |
| 引用、版规、发送预览 | 共用 TextWindow，自动统一 |
| 原图窗口 | 使用同一外壳，保留缩放、保存和下载取消逻辑 |

Windows 自身的文件选择、保存对话框和 MessageBox 仍使用系统样式。

## 设计和代码说明

- 子窗口标题栏高 44 DIP，窗口按钮为 44 × 44 DIP；与主窗口沿用相同的 Sidebar / Text / Line 语义颜色。
- 标题文字左侧留 16 DIP；标题随 Window.Title 绑定，避免两处名称不一致。
- 使用库提供的拖动、窗口按钮和非客户区处理；没有手写鼠标拖动或直接销毁窗口句柄。
- 标题栏和 Esc 都进入正常 Window.Close / Closing。模态子窗口保留 Owner，关闭后恢复主窗口交互。
- 派生窗口通过 SetClientContent 提供内部界面，共用外壳保留在外层，避免再次覆盖掉标题栏。
- 窗口尺寸继续受工作区和 DPI 约束；未改变正文、饼干、发言接口或持久化结构。

实现入口：[ChildWindow.cs](../src/Tide.Desktop/ChildWindow.cs)、[Dialogs.cs](../src/Tide.Desktop/Dialogs.cs)。

## 技能应用与复现

使用 `ui-ux-pro-max` 的一致性、浅深色配对、可见关闭入口和键盘退出要求；沿用已经验证的 WPF UI 方向，没有重新生成视觉方案。

同时使用 `diagnosing-bugs` 的实际场景复现和回归闭环。该问题是 preview.1 文档已记录的未迁移范围，原因直接可确认，因此省略不必要的多假设推演与历史二分。仍先运行真实窗口检查，再修改应用代码。

已执行的最小回归命令：

```powershell
.\tests\Tide.Desktop.Tests\bin\Release\net10.0-windows\Tide.Desktop.Tests.exe --child-windows artifacts/child-windows-before-preview2
```

修正前八类窗口全部失败，报告逐项指出“仍使用系统标题栏，未显示 WPF UI 标题栏”。报告与原始截图保存在 `artifacts/child-windows-before-preview2/`。测试使用独立资料与固定数据，不依赖真实饼干或论坛发言。

## 验证结果

最终完整回归位于 `artifacts/ui-qa-0.2.0-preview2-final/`：

- 八类子窗口在浅色、深色下都有可见的新标题栏和窗口按钮，按钮颜色跟随主题。
- 默认宽度与最小宽度（普通弹窗 470 DIP、原图窗口 500 DIP）通过；标题栏拖动区域及最小化 / 最大化 / 关闭命中类型正确。
- 最大化、还原、最小化和真实关闭命令通过；Closing 取消关闭仍有效。
- 发串窗口写入测试文字后通过标题栏关闭，草稿实际写入隔离资料目录。
- ShowDialog 打开期间主窗口被禁用；通过新标题栏关闭后，主窗口恢复可交互。
- 主窗口、两种弹窗、30 组工作区 / DPI 尺寸计算、饼干 / SAGE、输入框、颜文字与字号既有回归均通过。
- Release 构建 0 警告、0 错误，18 项核心行为测试通过。

最大化测试还修正了测量方式：FluentWindow 的 DWM 外框可能包含屏幕外的不可见缩放边框。在当前 125% 缩放下，它多出每边 9 个物理像素。现在以实际客户内容边界判断最大化布局，并检查三个完整按钮都位于工作区内；报告用 FrameSource 区分测量来源，不通过裁剪矩形掩盖按钮越界。

最终单文件 EXE 的独立窗口检查位于 `artifacts/window-published-0.2.0-preview2-final/`。通过 UI Automation 实际打开发串、嵌套饼干管理和阅读设置，确认三个完整窗口按钮位于工作区内，关闭后父窗口恢复操作，并保存对应截图。查找模态窗口时按进程枚举 HWND，再读取可访问性控件，不假定 OwnedWindow 必须位于桌面可访问性树的直接下一层。

最终 EXE 的联网只读检查两次遇到 HTTP 502，停在时间线加载阶段，未完成公开读串和图片回归。失败记录位于 `artifacts/release-qa-0.2.0-preview2/` 与 `artifacts/release-qa-0.2.0-preview2-final/`；没有把旧版本的联网结果当作本轮通过记录。用户提供站点维护信息后，停止等待接口恢复。

上述窗口检查在当前机器上实测，不等同于跨显示器或另一台干净机器验证；没有发送真实发言。

## 运行与交付

新版本位于 `release/0.2.0-preview.2/Tide.exe`，设置页和状态栏显示“预览 2”。preview.1 和 0.1.3 均保留，未替换正在运行的旧程序。

默认仍使用 `%LOCALAPPDATA%\TideIsland`。退出旧版后再打开 preview.2，即可沿用原数据；需要并行对照时使用 `--data-dir` 指定独立目录。

许可与哈希随版本目录提供，最终校验值见 [verification.md](verification.md)。

