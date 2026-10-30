# 0.2.0-preview.5 · X 岛启动通知

日期：2026-10-22。按用户提供的通知弹窗参考实现。

## 来源与行为

雾岛和霞岛通过启动时 GET 独立的 [通知 JSON](https://nmb.ovear.info/nmb-notice.json) 获得站点公告。源码、字段和去重机制见 [接口调研](notifications-research.md)。通知内容由该源站提供，客户端不生成或猜测维护信息。

- 主窗口显示后检查一次，与目录 / 首页并行。目录失败不会使通知检查被跳过。
- `enable=true` 且正文有效时显示“来自 X 岛的通知”小窗；`enable=false`、空正文、超时或接口异常均不弹窗。
- 普通确认或 X / Esc 仅关闭本次。只有勾选“不再显示该条通知”并按“确定”，才保存当前通知的免打扰记录。
- 去重键是原始 `date` 字段和转换后正文的 SHA-256。`date` 保留为版本字符串，不误当 Unix 时间戳；版本或正文更新均会产生新提示。
- 仅存储最近 100 个已忽略摘要，不缓存公告正文，因此不会在断网时重新弹出旧维护信息。
- 用户正在其他窗口操作、主窗口最小化或不在前台时，延后至主窗口可交互再显示。退出客户端时取消请求及待显示通知。

## 窗口设计与技能

继续使用 `ui-ux-pro-max`，针对 WPF 查询 `dialog keyboard focus asynchronous --stack wpf`；使用 `research` 通过研究子代理核对一手源码，记录独立接口和客户端差异。

沿用项目 WPF UI 子窗口标题栏与浅深色资源。默认 560 × 570 DIP，最小 420 × 360 DIP，并服从工作区尺寸限制。正文可滚动与复制，底部复选框和确认按钮固定可见；确认按钮默认获得焦点并支持 Enter，Esc 走正常关闭流程。通知中的换行标签转换为换行，最终正文使用 WPF Run 显示纯文本。

截图：[浅色测试](../artifacts/ui-qa-0.2.0-preview5/notice-light.png)、[深色测试](../artifacts/ui-qa-0.2.0-preview5/notice-dark.png)、[窄窗口长正文](../artifacts/ui-qa-0.2.0-preview5/notice-long-narrow.png)、[最终 EXE 的真实通知](../artifacts/window-published-0.2.0-preview5/site-notice.png)。前三张是固定测试内容，最后一张来自本次匿名联网响应。

## 实现入口

- `SiteNoticeClient`：独立无 Cookie 的 HTTP 客户端，8 秒超时，不重试、不自动跟随重定向，限制解压后响应最多 64 KiB，校验 JSON 字段并转换正文。
- `UserState.SuppressedNoticeKeys` / `LocalStore`：兼容旧资料、null 和重复记录，保存已忽略摘要。
- `StartupNoticePresenter`：每次启动只检查一次，控制前台呈现、模态窗口和关闭取消；通知失败不覆盖读串错误。
- `SiteNoticeWindow`：可复制正文、固定操作区以及仅确认时才生效的免打扰选项。
- `App.OnStartup`：正常启动并行加载目录 / 首页和通知；现有读串截图模式跳过通知以维持自动化流程。

对应源码：[通知接口](../src/Tide.Core/SiteNoticeClient.cs)、[启动流程](../src/Tide.Desktop/StartupNoticePresenter.cs)、[弹窗](../src/Tide.Desktop/SiteNoticeWindow.cs)、[程序入口](../src/Tide.Desktop/App.xaml.cs)。

## 验收

- 24 项核心测试通过，覆盖通知端点与匿名请求、字段与换行、关闭开关、空正文、畸形 JSON、超大响应、错误状态与重定向、取消、版本 / 内容变化和旧资料兼容。
- 完整源码界面回归通过。新增通知检查覆盖浅深色、长正文、底部按钮可见、直接关闭不保存、确认后持久保存、状态重新载入、更新公告重新提示、同次启动只检查一次、失败不打断及退出取消。
- 最终单文件 EXE 通过真实启动验收，自动显示源站维护通知，文字与本次 HTTP 200 返回内容相符；勾选免打扰后保存成功，并恢复主窗口操作。随后发串、饼干、设置及版块收藏检查通过。
- 本轮没有重跑完整公网读串验收，也没有导入真实饼干或提交发言。

报告目录为 `artifacts/ui-qa-0.2.0-preview5/` 与 `artifacts/window-published-0.2.0-preview5/`；版本、大小及哈希见 [交付验证](verification.md)。新版为 `release/0.2.0-preview.5/Tide.exe`，运行前退出旧版。preview.4 保留。
