# preview.20：串内官方 tips

日期：2026-10-27。对应 TODO 第 6 项，待用户验收。第 7 项彩色 emoji 已获用户确认，TODO 已划掉；大型颜文字图案的严格对齐继续暂缓。

后续外观已在 [preview.21](tips-refinement-preview21.md) 改为普通分隔线，下方保留 preview.20 交付时的实现记录。

## 行为与界面

- 官方 tips 随串页返回，按原始位置显示在回复之间；同页多个提示、不同页相同提示 ID 都会保留。未收到提示时不额外生成内容。
- 采用紧凑的淡色背景和 `Tips` 标签，适配浅色、深色主题。正文沿用现有文字、链接、图片与 Windows 彩色 emoji 显示方式。
- 隐藏提示的伪楼号、2099 年日期、饼干标记和引用按钮。只看 PO 时仍显示官方提示。
- 正常回复的页数、索引和去重不受提示影响。跳页替换当前提示集合，前插/后续页沿用原有位置保持机制。
- 提示不作为持久化的阅读位置；保存、恢复及删除回复后的近邻定位只使用真实楼层。
- tips 随正文页一起缓存，网络失败时可离线显示。旧缓存缺少 tips 字段也能读取，联网刷新后才补入当次服务端投放内容。

## 来源与根因

本次核对公开接口 `https://api.nmb.best/api/thread?id=51288554&page=1`。响应的 `Replies` 中包含特殊条目，识别字段为：

| 字段 | 值 |
| --- | --- |
| `id` | `9999999` |
| `user_hash` | `Tips` |
| `now` | `2099-01-01 00:00:01` |

旧实现将 ID 为 `9999999` 的条目当作占位数据直接过滤，因此接收后没有进入阅读界面。现在同时匹配上面三个字段，不依赖可修改的标题或名称。只有部分字段匹配时仍作为普通回复处理。

识别约定参考开源手机端雾岛的 [NmbTips.kt](https://github.com/123Duo3/XDNMB/blob/main/shared/src/commonMain/kotlin/ink/duo3/fogisland/shared/util/NmbTips.kt) 和 [NmbTipsTest.kt](https://github.com/123Duo3/XDNMB/blob/main/shared/src/commonTest/kotlin/ink/duo3/fogisland/shared/util/NmbTipsTest.kt)，并以实际接口响应核对。实现未复制对方代码。当前约定以服务端返回这些标记为前提，未来若更改需相应更新。

这里的 tips 属于阅读页内容，和独立获取、启动时弹窗的站点公告分别处理。

## 实现与技能

本次采用 `diagnosing-bugs` 定位旧过滤逻辑，并应用 `ui-ux-pro-max` 的桌面信息层级、主题可读性及现有交互一致性建议。技能文件位于 `diagnosing-bugs` 和 `ui-ux-pro-max`。

- `Tide.Core/Models.cs`：`Post.IsSiteTip` 集中识别；`ThreadTip` 保存正文及其前方真实回复数量。
- `Tide.Core/XIslandApi.cs`：解析时拆分正常回复与提示，保留相对位置。高页校验仍只比较正常回复，避免随机 tips 干扰服务端截页检测。
- `MainViewModel.IncrementalLoading.cs`：按页合并显示项；正常回复去重、tips 保留各页独立实例。
- `MainWindow.xaml`：提示专用视觉状态，复用正文渲染与安全链接处理。
- `ReadingAnchor` / `ReadingProgressTracker`：持久化阅读进度跳过提示；短暂的前插视口锚点仍可使用可见提示对象。

## 验证与验收

新增核心检查先复现提示丢失和单独按 ID 误过滤，再修复实现。当前核心检查 36/36 通过，覆盖提示位置、多提示、部分标记误匹配、缓存往返和旧缓存兼容。

WPF 专项 `artifacts/tips-preview20-r2/` 检查了浅色/深色、原始顺序、同页多条、跨页同 ID、只看 PO、安全链接、无引用/身份、进度不变、分页、跳转、离线缓存以及换串清理。跳页、阅读进度和连续加载原有专项也通过。本轮测试使用隔离资料，没有发帖或修改真实订阅。最终发布包检查、哈希与空间占用见 [交付验证](verification.md)。

手动验收：

1. 退出旧版，打开 `release/0.2.0-preview.20/Tide.exe`。
2. 打开普通串并浏览回复；服务端投放时可看到 `Tips` 淡色区块。可用 `51288554` 作参考串，内容由服务端决定。
3. 切换只看 PO、翻到其他页、重新打开串，检查提示位置及正常阅读进度；浅色和深色均可查看。

第 6 项在用户确认后再划掉。没有将其他 TODO 项标为完成。
