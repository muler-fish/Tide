# 潮汐 0.1 · 桌面阅读设计

本文保留首版设计记录。2026-10-21 的 WPF UI 改造见 [0.2.0 试用版设计与验证](wpf-ui-preview-0.2.0.md)，其中更新了控件依赖、主题、标题栏和阅读布局。

目标：日常可用的 X 岛 Windows 客户端。用户最初想复刻老 A 岛 Windows 应用；尚未确认具体版本，因此首版沿用摸鱼岛的三栏信息结构，独立实现。

- WPF / .NET 10 原生桌面；不依赖浏览器内核，无外部 UI 库。
- 左侧版块与个人收藏、中间串列表、右侧正文。分栏可拖动调整。
- 青绿强调色、柔和灰绿侧栏、白色阅读面；深色主题同样保留足够对比度。
- 中文正文使用系统 Microsoft YaHei UI，16px 起，可调字号。不加载远程字体。
- UI/UX skill 搜索中的极简桌面风格适用；营销页 Hero 与西文字体推荐不适用于匿名版，未采用。
- 网络操作异步；请求取消并隔离过期结果；列表虚拟化；错误可重试。
- 图片按需加载；正文不执行 HTML、JavaScript 或 XAML；引用和外链用显式交互打开。
- 不自动发言、不自动轮询；发言先预览再手动发送，网络异常不重试 POST。
- 饼干以 Windows 当前用户 DPAPI 加密保存；界面只显示备注，不展示密钥。

参考：
- https://github.com/junbluedd/Moyu-Island （2026-10-19 查询时仓库只有 README、截图等，未提供应用源码；仅参考行为，不复用代码或图片）
- https://www.nmbxd1.com/t/69574374
- https://www.nmbxd1.com/t/69514846
- https://www.nmbxd1.com/t/69092642
- https://github.com/cordear/Adnmb_UWP （发现的旧客户端候选，尚不能确认是用户所指应用）
- https://github.com/123Duo3/XDNMB （雾岛，Kotlin 跨平台；核对 network/api/NmbApiClient.kt 中的接口、multipart 字段、water 省略语义、HTML 成功提示；独立实现，未复制代码）
- https://github.com/orzogc/xdnmb （霞岛，Flutter 开源客户端，供后续交互参考）

接口核对：读取 api.nmb.best 的 getForumList、getTimelineList、timeline、thread、getCDNPath，并核对主站版块页表单。没有为测试提交真实发言。
