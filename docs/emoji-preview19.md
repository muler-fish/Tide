# preview.19：Windows 彩色 emoji

日期：2026-10-26。用户于 2026-10-27 确认 TODO 第 7 项通过，已划掉；大型颜文字图案的严格对齐按用户要求暂缓。沿用统一 MiSans。

## 显示与交互

- 正文、引用/发送预览、通知、串列表和标题、颜文字菜单、发言正文输入区使用 Windows 彩色 emoji。
- 读取系统 Segoe UI Emoji，不随包附带 Apple、Fluent 3D 或其他 emoji 字库。字形覆盖取决于 Windows 字体版本；国旗由 Emoji.Wpf 提供小型矢量回退。
- ``↙(`ヮ´ )↗ 开摆！`` 中的箭头保持普通符号。默认文本字符需要 VS16 才采用表情形式，例如 `↗️`；VS15 明确要求文本外观。
- 深浅主题保持 emoji 原色，字号随正文缩放。复制、选区、草稿和提交仍使用原始 Unicode，保留换行及尾部空行。
- 标题/名称输入框继续使用普通文本编辑，发言预览和阅读显示可彩色化。没有更改发帖接口。

## 实现与排查

应用了 `diagnosing-bugs` 和 `ui-ux-pro-max` 技能，位置为 `diagnosing-bugs` 与 `ui-ux-pro-max`。先用同一个 😀 复现旧正文控件彩色像素为 0，确认 WPF 的原文字绘制路径没有呈现字体颜色层，再检查深浅主题与各显示入口。

固定依赖 `Emoji.Wpf 0.3.4`。`ColorEmoji` 将识别出的序列转换为颜色图形，保留周围 HTML 样式、引用和安全链接；隐藏内容展开后才渲染。普通文字继续用 MiSans 和原有字体回退。

`EmojiSequences` 从 Unicode 17.0 数据保留 5,009 条完全限定序列及 ZWJ/keycap 兼容写法，最长优先匹配。这样不会像原库一样把裸箭头转成表情，也能识别其旧数据缺失的新 emoji。数据和许可来源见 `licenses/Emoji-NOTICE.txt`。

截图核对时发现家庭组合拆分；Windows 与库虽然取得相同字形编号，但库未正确处理新版字体的组合叠放位置。`WindowsEmojiDrawing` 对 ZWJ 组合使用 Windows 原生排字的字形、推进量、偏移，再按同一字体的 COLR/CPAL 颜色层绘制并缓存。系统缺字时沿用回退，不补装字库。

`EmojiEditor` 由原生 RichTextBox 管理焦点、选区和撤销，`EmojiText` 在颜色图形与 UTF-16 原文位置间转换。仅重建 emoji 序列变化的段落；输入法组字期间不重排，候选确认留给输入法。粘贴只接受纯文本并检查长度。`EmojiLabel` 让列表与菜单采用相同的识别规则。

## 体积与验证

按十进制 100 MB（100,000,000 字节）计算。打包脚本检查整个发布目录；最终 EXE 检查还计入首次启动展开的原生运行时。实际字节数、SHA-256 和检查结果见 [交付验证](verification.md)。预算不含个人资料、后续阅读缓存、下载图片、历史版本和开发文件；MiSans 原始压缩包及 Global 全字库不打包。

自动检查覆盖彩色像素、字号、主题、跨行选区、换行、撤销/重做、组合序列、箭头、呈现选择符、近一万字输入，以及最终 EXE 草稿保存后重新打开。使用隔离资料，没有读取个人饼干或真实发帖。原生输入法候选操作的实际手感仍需用户验收。

验收时可在正文输入 `😀 ❤️ 👍🏽 👨‍👩‍👧‍👦 👩🏽‍💻 🫠` 和 ``↙(`ヮ´ )↗ 开摆！``，检查颜色、箭头、中文输入、复制粘贴、撤销、保存关闭后重新打开；不需要发送。

参考：[Emoji.Wpf 源码](https://github.com/samhocevar/emoji.wpf)、[0.3.4 版本](https://www.nuget.org/packages/Emoji.Wpf/0.3.4)、[Unicode 17.0 数据](https://github.com/unicode-org/unicodetools/blob/main/unicodetools/data/emoji/17.0/emoji-test.txt)。
