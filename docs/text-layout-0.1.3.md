# 潮汐 0.1.3 · 输入框与正文排版修复

日期：2026-10-20。对应用户截图中的“搜索输入显示不全”和“行间距太大了”。

## 技能与方法

本轮使用 `ui-ux-pro-max` 的文字重排、行高及 WPF 样式规则，并用 `diagnosing-bugs` 建立失败检查、缩小复现、逐项修复。

执行的定向查询为 `text clipping line spacing --domain ux` 和 `text input padding layout --stack wpf`。采用内容可见、避免固定区域裁字、正文采用适当行高的建议；保留现有三栏结构与配色。

最小复现使用实际主窗口的 JumpBox、实际 PostBody 控件及固定文本，分别测量字符与输入视口的边界、正文渲染高度。原版检查确实失败，再分别验证内边距和换行修正，最后调整行高。

## 根因与修复

| 问题 | 根因 | 修复 |
| --- | --- | --- |
| 跳串文字下半部分被裁切 | WPF 已将 TextBox.Padding 传给 PART_ContentHost；自定义模板又把同一份 Padding 设置成宿主 Margin，重复扣除可用高度 | 移除重复 Margin，提示文字保留一次内边距；跳串文字垂直居中 |
| 正文、颜文字行间隔过大 | API 常用 `<br />` 加源码换行符表示一次换行；原渲染器分别生成 LineBreak 和含换行的 Run，形成额外空行 | 在共享正文归一化中去掉换行标签后的一次源码换行 |
| 正文稍显松散 | 原正文行高为字号的 1.65 倍 | 收紧至 1.5 倍，并继续随设置中的正文字号变化 |

输入框修复应用于共用模板，顶部跳串、列表查找、页码和弹窗输入框都不再重复计算内边距。没有通过截断输入或缩小文字掩盖问题。

换行处理兼容 `<br>`、`<br />`、大小写、CRLF、整体转义的 HTML，以及段落 / div 结束标签。仅消除紧随这些换行标签的一次源码换行；保留连续 `<br>`、纯文本空行、下一行的普通空格和全角缩进。正文安全解析及链接处理保持原有流程。

## 实测结果

当前屏幕 1920 × 1080，Windows 缩放 125%。尺寸均为 WPF DIP。

| 测量项 | 修复前 | 最终版本 |
| --- | --- | --- |
| 顶部输入文字所需高度 | 16.8 | 16.8 |
| 输入框实际可见文字高度 | 9.6 | 16.8 |
| 输入框整体高度 | 43.2 | 34.4 |
| 正文 `第一行<br />第二行` | 52.8 | 48 |
| 正文 `第一行<br />\n第二行` | 79.2，错误多一行 | 48，与正常两行一致 |
| 有意空行 `第一行<br /><br />第二行` | 79.2 | 72，仍保留空行 |

读取了公开串 No.69655993 中的回复 No.69656047，并将用户截图对应的原始颜文字保存为离线夹具。最终显示为 10 行，16 DIP 字号下高度 240 DIP；13 / 16 / 24 DIP 字号的高度依次为 195.2 / 240 / 360 DIP。已查看浅色与深色截图，缩进和原有空行规则保持可用。

- Release 构建无警告、无错误。
- 核心行为测试 18 / 18 通过，包括新加入的 HTML 换行、转义、缩进和空行用例。
- WPF 排版回归通过；窗口边界、分组、饼干 / 红名 / SAGE、主题和引用弹窗回归也通过。
- 原始失败证据：`artifacts/text-layout-before-0.1.3/`。
- 单独验证内边距：`artifacts/text-layout-padding-0.1.3/`。
- 单独验证换行：`artifacts/text-layout-newlines-0.1.3/`。
- 最终排版、字号及主题报告和截图：`artifacts/ui-qa-0.1.3/`。

## 源码与复现

- [App.xaml](../src/Tide.Desktop/App.xaml)：输入框内容宿主模板。
- [MainWindow.xaml](../src/Tide.Desktop/MainWindow.xaml)：跳串文字垂直对齐。
- [ContentText.cs](../src/Tide.Core/ContentText.cs)：HTML 源码换行归一化，保留中文注释。
- [PostBody.cs](../src/Tide.Desktop/PostBody.cs)：统一的行高系数。
- [核心测试](../tests/Tide.Tests/Program.cs)、[WPF 排版测试](../tests/Tide.Desktop.Tests/TextLayoutChecks.cs)、[颜文字夹具](../tests/Tide.Desktop.Tests/Fixtures/ascii-post.html)。

```powershell
dotnet build Tide.slnx -c Release
dotnet tests/Tide.Tests/bin/Release/net10.0/Tide.Tests.dll

# 只检查排版，不依赖网络
.\tests\Tide.Desktop.Tests\bin\Release\net10.0-windows\Tide.Desktop.Tests.exe --layout artifacts/text-layout-check

# 窗口、标记、主题与排版的完整回归
.\tests\Tide.Desktop.Tests\bin\Release\net10.0-windows\Tide.Desktop.Tests.exe artifacts/ui-qa-0.1.3
```

最终程序路径、校验值与只读联网检查见 [交付验证](verification.md)。未改动系统分辨率或缩放，字号验证不等同于在所有物理显示器配置上实测。
