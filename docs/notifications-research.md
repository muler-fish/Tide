# X 岛站点通知接口调研

核对日期：2026-10-22。使用 `research` 技能，依据公开源码与实际匿名 HTTPS 请求；本文件记录协议和第三方客户端做法，实施与最终验收另见版本交付文档。

## 结论

雾岛与霞岛均主动 GET 同一个独立 JSON 地址：[`https://nmb.ovear.info/nmb-notice.json`](https://nmb.ovear.info/nmb-notice.json)。这是一份站点公告，由客户端启动后拉取、判断是否显示；所核对实现没有使用 WebSocket、系统推送订阅或饼干认证来接收这份公告。它与 `api.nmb.best` 的版块、时间线接口分开，因此可以在主站维护时仍提供维护说明。[雾岛端点与 getNotice](https://github.com/123Duo3/XDNMB/blob/42cff80eb77a35898ead718d12fe39cfd41ba7e3/shared/src/commonMain/kotlin/ink/duo3/fogisland/shared/network/api/NmbApiClient.kt#L60-L61)、[霞岛 URL](https://github.com/orzogc/xdnmb_api/blob/28fdba53031a15e0de88954679afe91b9308f71a/lib/src/urls.dart#L32-L33)、[霞岛匿名 GET](https://github.com/orzogc/xdnmb_api/blob/28fdba53031a15e0de88954679afe91b9308f71a/lib/src/xdnmb.dart#L1995-L1999)。

这里的“通知”指全站公告，不包含回帖提醒、订阅更新或私信。截图中的网页维护提示也不需要通过抓取整张网页来获得；独立通知 JSON 已包含对应维护文字。

## 协议与当前响应

实际请求返回 HTTP 200，包含下列字段。响应快照保存在 `artifacts/notifications-research/notice-live.json`；下表内容和版号仅代表本次请求时的结果。[通知端点](https://nmb.ovear.info/nmb-notice.json)

| 字段 | 本次值 / 类型 | 用途 |
| --- | --- | --- |
| `content` | 字符串，含 `<br>` | 通知正文，本次是 2026-10-21 维护说明及官方动态渠道提示 |
| `date` | 数字 `2026102100008` | 通知日期与序号，不是可直接套用的 Unix 时间戳 |
| `enable` | 布尔值 `true` | 是否启用这条通知 |

雾岛 DTO 对应可空 `String`、`Long` 和 `Boolean`。仓储只在 `enable == true` 且去除首尾空白后的正文非空时返回通知。当前响应及所查模型没有独立 `id`、强制展示标志或过期时间字段；不应自行把 `enable` 解释为“无视用户忽略设置”。[NoticeDto](https://github.com/123Duo3/XDNMB/blob/42cff80eb77a35898ead718d12fe39cfd41ba7e3/shared/src/commonMain/kotlin/ink/duo3/fogisland/shared/network/model/NoticeDto.kt)、[雾岛有效性判断](https://github.com/123Duo3/XDNMB/blob/42cff80eb77a35898ead718d12fe39cfd41ba7e3/shared/src/commonMain/kotlin/ink/duo3/fogisland/shared/repository/ForumRepository.kt#L145-L163)。

霞岛底层模型把 `date` 转成字符串，前 8 位按 `yyyyMMdd` 解析，后续数字作为公告 `index`。因此本次值对应 2026-10-21、序号 8。雾岛另有紧凑日期及 Unix 秒 / 毫秒兼容解析；两者不应被混淆为服务端已公开保证的统一时间协议。客户端去重宜保留原始 `date`，不要只保留解析后的日期或套用 Unix 毫秒。[霞岛 Notice 模型](https://github.com/orzogc/xdnmb_api/blob/28fdba53031a15e0de88954679afe91b9308f71a/lib/src/xdnmb.dart#L37-L68)、[雾岛日期兼容逻辑](https://github.com/123Duo3/XDNMB/blob/42cff80eb77a35898ead718d12fe39cfd41ba7e3/shared/src/commonMain/kotlin/ink/duo3/fogisland/shared/util/NmbDateTime.kt#L81-L93)。

## 已核对的客户端行为

### 雾岛

浏览视图模型初始化时读取已忽略的通知内容和本地目录缓存，再刷新目录。版块、时间线和通知各自以异步任务发起；目录读取失败时仍会单独读取通知结果并更新界面。网络通知获取失败时可保留已有通知，而 `enable=false` 得到的是无有效通知。[ForumBrowseViewModel 初始化与刷新](https://github.com/123Duo3/XDNMB/blob/42cff80eb77a35898ead718d12fe39cfd41ba7e3/androidApp/src/main/kotlin/ink/duo3/fogisland/viewmodel/ForumBrowseViewModel.kt#L130-L145)、[并发及失败处理](https://github.com/123Duo3/XDNMB/blob/42cff80eb77a35898ead718d12fe39cfd41ba7e3/androidApp/src/main/kotlin/ink/duo3/fogisland/viewmodel/ForumBrowseViewModel.kt#L266-L297)。

其去重键是纯文本正文去除首尾空白后的值：`dismissSiteNotice` 仅记住本会话关闭的内容，`dismissSiteNoticeUntilChanged` 同时持久化忽略内容。新正文与已忽略正文不同则能再次显示，不是关闭全部未来通知。[关闭及持久化](https://github.com/123Duo3/XDNMB/blob/42cff80eb77a35898ead718d12fe39cfd41ba7e3/androidApp/src/main/kotlin/ink/duo3/fogisland/viewmodel/ForumBrowseViewModel.kt#L147-L162)、[可见性去重](https://github.com/123Duo3/XDNMB/blob/42cff80eb77a35898ead718d12fe39cfd41ba7e3/androidApp/src/main/kotlin/ink/duo3/fogisland/viewmodel/ForumBrowseViewModel.kt#L357-L373)。

### 霞岛

初始化服务获取通知并保存；通常一次，首次运行有最多 5 次重试逻辑。列表首帧后安排显示，显示函数会等待通知获取完成和导航器可用。有效通知正文或日期变化时，保存并重新启用显示开关。[初始化获取](https://github.com/orzogc/xdnmb/blob/2467f43d58b837c02b1fe72da1c7ca6484ab50d9/lib/app/data/services/xdnmb_client.dart#L211-L251)、[列表安排显示](https://github.com/orzogc/xdnmb/blob/2467f43d58b837c02b1fe72da1c7ca6484ab50d9/lib/app/modules/post_list.dart#L1889-L1905)、[保存及等待](https://github.com/orzogc/xdnmb/blob/2467f43d58b837c02b1fe72da1c7ca6484ab50d9/lib/app/data/services/persistent.dart#L116-L151)。

弹窗使用可滚动正文和“不再提示此条公告”复选框，点击确定后将显示开关设为复选框状态的反值。当前版本弹窗标题是“公告”及日期；不能仅凭用户截图断定截图来自霞岛。[NoticeDialog](https://github.com/orzogc/xdnmb/blob/2467f43d58b837c02b1fe72da1c7ca6484ab50d9/lib/app/widgets/dialog.dart#L160-L281)。

### 摸鱼岛

本次读取 `junbluedd/Moyu-Island` 的 `main` 树，只有 README、截图和占位文档，未找到可核对的客户端源码，因此没有把其通知实现当作证据。[核对提交的文件树](https://github.com/junbluedd/Moyu-Island/tree/4a2b08ddf7874ec03a28778d6720e4f9f9bcec97)。

## 对潮汐的实施建议

以下是本项目的设计选择，不是服务端协议新增字段：

1. 主窗口出现后异步检查一次通知，与首页读取独立；通知请求不携带饼干，不因目录 HTTP 502 而跳过。
2. 仅对启用且正文非空的通知显示小窗，正文可滚动，底部保留“不再显示该条通知”和确定按钮；X / Esc 可关闭本次窗口。
3. 以原始 `date` 和正文指纹共同识别一条通知。勾选后只忽略当前内容；版号变化或同版号内容修正时仍提醒。
4. 不执行通知中的 HTML / JavaScript；可复用现有受限正文渲染。失败、超时或无通知时不阻塞首页。
5. 独立测试 `enable=false`、空正文、异常 JSON、日期数字 / 字符串、关闭与持久忽略、同版号内容变化、窗口关闭时取消请求，以及主读接口失败但通知成功。

## 复核说明

源文件取自上方固定提交，并留在 `artifacts/notifications-research/` 便于本轮核对。公开端点未使用身份凭据。Windows curl 在本环境发生 `CRYPT_E_REVOCATION_OFFLINE`，沙箱外重试仍失败；随后 PowerShell `Invoke-WebRequest` 使用正常 HTTPS 校验成功，未设置忽略证书错误或跳过 TLS 验证。该网络工具差异不代表通知服务存在证书漏洞。
