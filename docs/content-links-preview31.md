# 正文与 tips 站内链接 · preview.31

后续交互已在 [preview.32](link-preview-preview32.md) 改为串预览和板块确认，下面保留 preview.31 的实现记录；不再以直接跳转作为当前行为。

对应 TODO 第 9 项（原 20）。用户要求先继续、下次验收，因此第 8 项不归档，本项完成后也保留待验收。本轮遵循最小测试原则。

## 点击行为

正文、tips、版规及引用小窗中的 HTTP/HTTPS 链接使用相同入口。地址决定目标，蓝字文案和 tips 的出现时间不影响识别。

| 链接 | 行为 |
| --- | --- |
| 已知域名的 `/t/串号` 或旧式 `Forum/thread` | 在串阅读区打开；无页码从第 1 页开始，有页码则直达该页 |
| `?page=2&r=回复号` 或数字回复锚点 | 打开指定页；目标回复存在时定位到该条，必要时解除 PO 筛选以显示目标 |
| `/f/板块名`、`Forum/showf` 或 `Forum/timeline` | 切换串列表区及导航选中状态，保留右侧已有正文；支持链接页码 |
| `>>No.串号` | 保持原有引用小窗，包括站内 a 标签中的引用文字 |
| 外站、未知站内页面、无法匹配的板块、不支持的参数/锚点 | 保留完整地址，交给默认浏览器 |
| `javascript:`、`file:` 等非网页协议 | 不作为可执行链接 |

跳串输入框同样保留串链接里的页码和回复参数；纯串号仍按原有规则恢复阅读进度。明确页码的板块链接显示实际页区间，避免把“正在第 2 页”误报为“已加载两页”。

## 识别范围

精确匹配 `nmbxd.com`、`www.nmbxd.com`、`nmbxd1.com`、`www.nmbxd1.com`；不使用域名包含判断。其他子域、相似域名、非默认端口及带用户信息的网页地址不接管。普通外链继续经系统浏览器打开。

支持新式 `/t/123`、`/f/婆罗门一`，及旧式 `/Forum/thread/id/123/page/2.html`、`/Home/Forum/showf/id/4/page/2.html`、`/Forum/timeline/id/1` 和查询参数式 `/Forum/showf?id=4&page=2`。HTML 中的根相对路径与 `//域名/路径` 会先正确补全；HTML 实体与中文 URL 编码各按所在层解码，避免把外站地址拼成本域路径。

板块 API 的 `name` 与 `showName` 可能不同。模型新增 `UrlName` 保存原名，显示仍使用 `Name`；例如网页 `/f/技术宅` 对应导航“技术宅(代码)”。旧资料缺少原名或链接指向新板块时，尝试刷新一次目录。仍找不到、目录不可用或页码超出该入口范围时回退网页，不猜测 ID。

站内请求失败在客户端沿用已有错误反馈，不另外打开浏览器。取消或迟到的目录请求不会覆盖后续手动选版；链接事件按所属主窗口分发，防止多个窗口重复处理。

## 证据与技能

2026-10-29 核对公开页面入口：[X 岛首页](https://www.nmbxd1.com/Forum)、[婆罗门一](https://www.nmbxd1.com/f/%E5%A9%86%E7%BD%97%E9%97%A8%E4%B8%80)、[技术宅](https://www.nmbxd1.com/f/%E6%8A%80%E6%9C%AF%E5%AE%85)、[综合线](https://www.nmbxd1.com/Forum/timeline/id/1)。部分工具返回缓存页面，仅据此核对路径形式，不据缓存内容判断当前站点状态。既有网页快照 `artifacts/large-thread-probe/site-page474.html` 和 `artifacts/emoticons-preview16-source/h.desktop.js` 提供 `r` 与 `page` 定位形式。

沿用 `ui-ux-pro-max`，本轮查询 `deep linking navigation feedback --domain ux`，采用 Navigation / Deep Linking 中“保留地址状态”的原则；结合 WPF 场景保留页码、楼层、加载反馈及原有引用交互，未采纳无关的移动端触感建议，也未重做页面风格。

## 实现位置

- `src/Tide.Core/IslandLink.cs`：纯链接解析、目标类型、页码/回复参数、板块目录匹配。
- `src/Tide.Core/Models.cs`、`XIslandApi.cs`：保存板块原名，兼容旧资料。
- `src/Tide.Desktop/PostBody.cs`：HTML 与纯文本链接入口、协议相对路径、引用区别。
- `src/Tide.Desktop/MainViewModel.Links.cs`：站内加载、未知板块目录刷新与回退决定。
- `src/Tide.Desktop/MainViewModel.cs`：显式页码和回复定位、板块页区间显示。
- `src/Tide.Desktop/MainWindow.xaml.cs`：所属窗口判断和浏览器交接。

## 最小验证与验收

只运行核心 `--links`、界面 `--links`，以及共用正文解析器直接受影响的 `--tips`、`--spoilers`。不运行全量测试、图片/订阅回归或完整 EXE 窗口巡检。具体结果和发布体积见 [交付验证](verification.md)。

验收时可点击 tips 中的婆罗门链接，或正文里的普通串链接、带页码链接、板块链接和外链；检查目标位置与打开方式。引用 `>>No.` 应仍显示小窗。第 8 项的看图功能继续等待另行验收。
