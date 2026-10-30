# X 岛订阅号与本地收藏

核对日期：2026-10-23。按 `research` 技能由独立研究任务读取公开源码。本文保留协议调研依据；后续用户明确要求直接移除串收藏，不做旧数据迁移。该决定已在 preview.9 实现，验证记录见 [通用订阅实现](subscriptions-preview9.md)。

## 结论

X 岛确实有可跨客户端使用的**订阅号**，接口字段名是 `uuid`。客户端把同一个订阅号交给 X 岛服务器，就能读取和修改它对应的串列表。雾岛和霞岛都提供订阅号设置，并调用同组 `Api/feed`、`Api/addFeed`、`Api/delFeed` 接口。这是服务端列表，不是只在设备上保存的书签，也不是网站维护公告的通知接口。[雾岛接口](https://github.com/123Duo3/XDNMB/blob/42cff80eb77a35898ead718d12fe39cfd41ba7e3/shared/src/commonMain/kotlin/ink/duo3/fogisland/shared/network/api/NmbApiClient.kt#L142-L160)、[霞岛接口 URL](https://github.com/orzogc/xdnmb_api/blob/28fdba53031a15e0de88954679afe91b9308f71a/lib/src/urls.dart#L153-L189)。

接入这套接口后，串入口命名为“我的订阅”，操作命名为“订阅 / 取消订阅”。preview.8 的旧实现仅保存本机串收藏；preview.9 已移除这套功能，改为真实云端增删与读取。版块星标仍是本机导航快捷项。[状态模型](../src/Tide.Core/Models.cs)、[订阅操作](../src/Tide.Desktop/MainViewModel.Subscriptions.cs)。

## 订阅号从哪里来

订阅号由客户端本地生成并保存；所查实现没有“注册订阅号”请求。霞岛首次运行时生成 UUID v4，设置中也可以手动输入现有 ID 或重新生成。雾岛生成 12 位小写字母 / 数字串，并支持输入已有 ID。因此参数虽然叫 `uuid`，不等于服务器只接受标准 UUID 格式。[霞岛首次生成](https://github.com/orzogc/xdnmb/blob/2467f43d58b837c02b1fe72da1c7ca6484ab50d9/lib/app/data/services/settings.dart#L883-L885)、[霞岛编辑与生成](https://github.com/orzogc/xdnmb/blob/2467f43d58b837c02b1fe72da1c7ca6484ab50d9/lib/app/modules/basic_settings.dart#L260-L329)、[雾岛生成与校验](https://github.com/123Duo3/XDNMB/blob/42cff80eb77a35898ead718d12fe39cfd41ba7e3/shared/src/commonMain/kotlin/ink/duo3/fogisland/shared/storage/preferences/ForumPreferences.kt#L20-L58)、[雾岛保存](https://github.com/123Duo3/XDNMB/blob/42cff80eb77a35898ead718d12fe39cfd41ba7e3/shared/src/commonMain/kotlin/ink/duo3/fogisland/shared/storage/preferences/ForumPreferences.kt#L175-L201)。

跨客户端使用方式是：从原客户端复制订阅号，在另一客户端的订阅号设置中输入相同值，再刷新订阅列表。可迁移的是服务器上的串成员；本机阅读进度、草稿、标签和缓存不会因为复制订阅号自动同步。雾岛切换 ID 会清空当前订阅缓存；霞岛也以 ID 为列表身份的一部分。[雾岛切换与缓存清理](https://github.com/123Duo3/XDNMB/blob/42cff80eb77a35898ead718d12fe39cfd41ba7e3/shared/src/commonMain/kotlin/ink/duo3/fogisland/shared/repository/ForumRepository.kt#L307-L310)、[霞岛列表身份](https://github.com/orzogc/xdnmb/blob/2467f43d58b837c02b1fe72da1c7ca6484ab50d9/lib/app/widgets/feed.dart#L38-L61)。

兼容性边界：雾岛会去除首尾空白、转为小写，并限制为最长 64 个字符；霞岛的编辑框没有同样的格式限制。这些是**客户端自身规则**，不能当成 X 岛服务端公开保证。潮汐新建时可采用随机 UUID v4；导入时不要默默截断或改写用户现有订阅号。[雾岛规则](https://github.com/123Duo3/XDNMB/blob/42cff80eb77a35898ead718d12fe39cfd41ba7e3/shared/src/commonMain/kotlin/ink/duo3/fogisland/shared/storage/preferences/ForumPreferences.kt#L20-L58)、[霞岛编辑框](https://github.com/orzogc/xdnmb/blob/2467f43d58b837c02b1fe72da1c7ca6484ab50d9/lib/app/modules/basic_settings.dart#L270-L295)。

## 接口与返回

下表依据实际客户端调用代码，路径相对于它们配置的 X 岛 API 主机。它们的 API 封装使用 GET；特别注意，`addFeed` / `delFeed` 即使通过 GET 访问也会修改远程数据，不能把这两个地址当普通网页探测。本轮没有访问任何增删接口，也没有读取他人的订阅号。[雾岛请求方法](https://github.com/123Duo3/XDNMB/blob/42cff80eb77a35898ead718d12fe39cfd41ba7e3/shared/src/commonMain/kotlin/ink/duo3/fogisland/shared/network/api/NmbApiClient.kt#L265-L275)、[霞岛调用](https://github.com/orzogc/xdnmb_api/blob/28fdba53031a15e0de88954679afe91b9308f71a/lib/src/xdnmb.dart#L2169-L2253)。

| 操作 | 已核对路径与参数 | 返回 / 边界 |
| --- | --- | --- |
| 获取列表 | `GET /Api/feed?uuid={订阅号}&page={页码}` | JSON 数组；页码从 1 开始。霞岛注释为每页最多 10 串，尚未用真实非空列表复测 |
| 添加 | `GET /Api/addFeed?uuid={订阅号}&tid={主串号}` | JSON 字符串；霞岛判断是否包含“订阅大成功” |
| 删除 | `GET /Api/delFeed?uuid={订阅号}&tid={主串号}` | JSON 字符串；霞岛判断是否包含“取消订阅成功” |

参数构造见[霞岛 URL 源码](https://github.com/orzogc/xdnmb_api/blob/28fdba53031a15e0de88954679afe91b9308f71a/lib/src/urls.dart#L153-L189)，校验、页大小说明和成功判定见[霞岛 API 源码](https://github.com/orzogc/xdnmb_api/blob/28fdba53031a15e0de88954679afe91b9308f71a/lib/src/xdnmb.dart#L2169-L2253)。本次没有验证 POST 兼容性；实现时可以继续核对服务端文档及独立测试号，不能把“应当用 POST”的设计惯例写成已验证协议。

订阅条目包含主串内容、`reply_count` 和 `recent_replies` 等字段；霞岛将 `recent_replies` 解码为回复 ID 列表，并注释最多 5 个。JSON 列表不是包含总页数的分页对象，也没有已同步阅读进度或未读计数。客户端可根据已读记录计算未读变化，但这是新增的本地逻辑，不是服务端推送。[Feed 模型与解码](https://github.com/orzogc/xdnmb_api/blob/28fdba53031a15e0de88954679afe91b9308f71a/lib/src/xdnmb.dart#L1331-L1442)。

设计上应逐页加载、按串号去重，不把“当前第一页没找到该串”判作“没有订阅”。本次未找到可靠的一手资料保证总订阅数上限、ID 长度上限或固定调用频率，不能自行宣称无限制；也未验证服务端是否对大小写等价处理。

## 与饼干和网页版订阅的关系

**通用订阅号不等同于饼干。** 这组 API 以独立的 `uuid` 选择列表，霞岛 `cookie` 参数是可选的，雾岛也只是按当前浏览身份附加可选 Cookie。凭据可能影响受限版块内容的可见性，但不会把订阅号变为发言身份。这里是从客户端代码确认的身份分离，没有通过更换真实饼干实验所有服务端权限行为。[霞岛可选 Cookie](https://github.com/orzogc/xdnmb_api/blob/28fdba53031a15e0de88954679afe91b9308f71a/lib/src/xdnmb.dart#L2174-L2183)、[雾岛可选 Cookie](https://github.com/123Duo3/XDNMB/blob/42cff80eb77a35898ead718d12fe39cfd41ba7e3/shared/src/commonMain/kotlin/ink/duo3/fogisland/shared/network/api/NmbApiClient.kt#L96-L100)。

**网页版还有另一套跟浏览饼干绑定的订阅。** 霞岛设置明确命名为“使用跟浏览饼干绑定的网页版订阅”；启用后，获取 / 增删切换到 `Forum/feed/page/...html`、`Home/Forum/addFeed/tid/...html`、`Home/Forum/delFeed/tid/...html`，不传上述 `uuid`。因此“同一饼干在网页上订阅的串”和“同一通用订阅号的串”不能默认是同一份列表。[霞岛开关说明](https://github.com/orzogc/xdnmb/blob/2467f43d58b837c02b1fe72da1c7ca6484ab50d9/lib/app/modules/basic_settings.dart#L334-L356)、[两套调用分支](https://github.com/orzogc/xdnmb/blob/2467f43d58b837c02b1fe72da1c7ca6484ab50d9/lib/app/data/services/xdnmb_client.dart#L114-L130)、[两套路径](https://github.com/orzogc/xdnmb_api/blob/28fdba53031a15e0de88954679afe91b9308f71a/lib/src/urls.dart#L153-L189)。

## 权限与隐私的实施含义

以下是由客户端请求结构推导的产品要求：所查通用订阅增删调用没有独立密码、签名或确认码，订阅号应按**能访问并修改该列表的秘密标识**对待，而不是可公开展示的昵称。只在用户主动查看 / 复制时显示全文；避免日志、异常、截图和遥测暴露带 `uuid` 的完整 URL；新建使用足够随机的值，不从饼干、用户名或机器序列号派生。[增删接口参数证据](https://github.com/123Duo3/XDNMB/blob/42cff80eb77a35898ead718d12fe39cfd41ba7e3/shared/src/commonMain/kotlin/ink/duo3/fogisland/shared/network/api/NmbApiClient.kt#L142-L160)。

换一个订阅号只是切换列表，不等于修改旧号、迁移旧内容或撤销旧号权限。雾岛的更改说明也只承诺清理缓存和切换列表。若以后提供迁移，需要显式执行逐串添加，保留失败记录，并与“导入已有订阅号”分开。[雾岛编辑说明](https://github.com/123Duo3/XDNMB/blob/42cff80eb77a35898ead718d12fe39cfd41ba7e3/androidApp/src/main/kotlin/ink/duo3/fogisland/ui/components/settings/SubscriptionUuidEditorDialog.kt#L99-L131)。

## 潮汐采用的方案

1. 移除串收藏的 UI、状态字段与操作，不提供旧收藏迁移。用户已明确当前项目没有用户，无需保留兼容路径。
2. 提供“订阅号”管理：新建、导入、显式复制；进入“我的订阅”刷新云列表，切号取消旧请求并拒绝迟到响应。
3. 星形按钮与 Ctrl+D 执行云端增删，收到明确成功响应后更新状态。结果不确定时先重新读取，不自动重试写入。
4. 版块收藏继续作为本机导航快捷项；阅读进度保存在本机历史。订阅成员不再额外保存为一份本机收藏。
5. 当前没有新回复推送，也没有自动同步阅读进度；通用订阅与启动站点公告相互独立。

## 核对范围与证据

本次通过正常 HTTPS 只读取得三个仓库的当前 HEAD，它们分别是：雾岛 `42cff80eb77a35898ead718d12fe39cfd41ba7e3`、霞岛 `2467f43d58b837c02b1fe72da1c7ca6484ab50d9`、霞岛 API 库 `28fdba53031a15e0de88954679afe91b9308f71a`。新增源码快照与 HEAD / 文件树位于 `artifacts/subscriptions-research/`；相同固定提交的既有接口文件保存在 `artifacts/notifications-research/`。

证据主要是公开客户端实现的一手源码，不是 X 岛服务端授权策略的官方规范。本轮未使用真实饼干、未枚举或读取他人订阅号、未创建远程订阅、未访问增删 API。服务端写入成功、分页真实容量和跨设备结果应在实现阶段使用专门测试号验证，不能列为本轮已完成的网络验收。
