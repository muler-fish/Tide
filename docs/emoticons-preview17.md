# preview.17 · 颜文字与发言编辑器调整

日期：2026-10-25。根据用户对 preview.16 的七项反馈重做。TODO 第 5 项仍待用户验收；第 3、4、10 项已验收，保持划掉状态。

## 界面与行为

1. 点击“颜文字”直接展开下拉菜单，上方“最近使用”最多 20 项，下方为完整字库。取消分类和搜索，不再打开独立选择窗口。
2. 单击即插入，保留正文光标与选区，插入可一次撤销。Esc 或点击菜单外关闭；菜单支持滚动和键盘方向导航。长图案用名称显示，悬停查看全文。
3. 最近使用按成功插入更新，重复项移到最前，超过 20 项淘汰最旧项。记录在本地状态文件中，重启恢复。超出正文长度限制时不插入，也不更新最近使用。
4. 骰子插入 `[1,100]`，可按 `[首项,尾项]` 修改范围。
5. 发串和回复均可填写标题、名称；顺序为标题 → 名称 → 正文。前两项放在默认收起的“可选项”中，正文直接显示。名称参与预览、草稿恢复和表单提交；留空仍可正常发言。
6. 回复窗口的目标串号放大到 20 DIP。仅调整显示，不改变回复目标。

## 资料来源

按网页优先、蓝岛补充的顺序整理，共 197 项，其中 32 项为多行图案。删除无意义的单个符号，保留全角空格与零宽空格。Gboard 字典不参与当前版本生成；资料副本仅作为历史来源保存，用户原文件未修改。

| 来源 | 去重后贡献 | 核对信息 |
| --- | ---: | --- |
| X 岛网页基础表 | 101 | [综合版1](https://www.nmbxd1.com/f/%E7%BB%BC%E5%90%88%E7%89%881)及 [h.desktop.js](https://www.nmbxd1.com/Public/Js/h.desktop.js)，2026-10-25 读取公开内容 |
| X岛-EX 网页扩展 | 92 | [上游脚本](https://github.com/SayaGoodBye/nmbxd-EX/blob/main/nmbxd-EX.user.js)，版本 4.2.0，blob `b754a2bd5fa2067caaf18f57b51655e6a2bf1bfe`。用户截图中的 neko2/3、冰封王座、冰箱先生3D 等来自该扩展 |
| 蓝岛资料 | 4 | 用户提供的《蓝岛颜文字.txt》；与网页重复的优先使用网页文本 |

只提取静态数据，没有执行网页或扩展脚本。X岛-EX 的元数据声明 WTFPL，来源说明随包放在 `licenses/EmoticonData-NOTICE.txt`。

生成器 `tools/Build-Emoticons.py` 使用去掉空白的比较键识别重复，不压缩保留下来的图案空格；同名多行图案优先采用网页版本。插入时只将换行统一为 Windows CRLF，不截断长图案。生成的 `Assets/emoticons.json` 内置在 EXE，使用时无需 Python、原始资料或联网。

一个兼容例外：名为 `҉( ﾟ∀。)` 的图案实际以“两个全角空格 + ҉ + 换行”起头，组合符号附着在全角空格上时，WPF 会选到不包含该字形的日文字体。生成字库时在组合符号之前加 U+200B 零宽基底（菜单名称同样处理），保留原缩进且不增加可见空格；原资料快照不改动，正文中已有的用户输入也不自动重写。

## 缺字修复

用户截图中的 `ᝫ` 数据完整，原字体无法提供字形；枪形颜文字则包含多个组合附加符号，原来的字体回退组合不能完整显示。通过真实 WPF 绘制检查复现了缺字 glyph 0。

`EmoticonFonts.cs` 与 `TideEmoticons.CompositeFont` 共用在正文输入、菜单、悬停预览和阅读正文。组合附加符号交给支持它们的系统字体，缺少的 Tagbanwa 与 Batak 字形由随包的 Noto 字体补齐。字体仅在程序内部使用，不安装或更改系统字体。

- [Noto Sans Tagbanwa](https://github.com/google/fonts/tree/main/ofl/notosanstagbanwa)：SIL OFL 1.1，字体 blob `88c192fe4b5f461d72f07d2def31618748babc41`。
- [Noto Sans Batak](https://github.com/google/fonts/tree/main/ofl/notosansbatak)：SIL OFL 1.1，字体 blob `2c4ce990273afd321e3c216852bf9b7d3125e95a`。
- 字体资源与复合字体的引用遵循 [Microsoft 的 WPF 字体打包说明](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/advanced/packaging-fonts-with-applications)。完整许可证随 EXE 分发。

本轮处理缺字，不将所有大型图案在不同设备上的严格对齐或彩色 emoji 记为完成；这些仍属 TODO 第 7 项。

## 技能、源码与验收

本轮继续使用 `ui-ux-pro-max` 处理下拉菜单、键盘和主题，使用 `diagnosing-bugs` 先复现缺字、再验证实际绘制结果。用户指定的简洁布局优先，不额外加入分类、搜索或说明文案。

- `EmoticonCatalog.cs` / `EmoticonMenu.cs`：字库、最近使用和下拉交互。
- `ComposerWindow.cs` / `Models.cs` / `LocalStore.cs` / `XIslandApi.cs`：可选名称、草稿兼容、预览和一次提交。
- `EmoticonChecks.cs`：真实菜单、选区、撤销、最近使用上限、取消、默认折叠、标题/名称顺序、骰子、草稿及长度限制。
- `EmoticonFontChecks.cs`：真实输入框及阅读区字形检查，字库逐项检查，长图案逐屏滚动检查。
- 最终构建与交付 EXE 记录见 [交付验证](verification.md)。自动测试使用隔离数据，不发布测试发言。

用户验收可依次检查：展开颜文字菜单并插入短/长图案；重开查看最近使用；试用截图中缺字的表情；展开可选项填写标题、名称再查看预览和草稿；点击骰子；打开“参与回应”确认串号大小。
