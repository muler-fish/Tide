# 第三方组件与资料

本文件记录源码和发布包中使用的第三方材料，不改变各自的许可。项目自身尚未指定开源许可证。

| 组件 / 资料 | 用途 | 许可或来源 |
| --- | --- | --- |
| WPF UI 4.3.0 | WPF 控件、主题和图标 | MIT；还原后 NuGet 包内的 LICENSE.md / ThirdPartyNotices.txt，由发布脚本复制 |
| ZXing.Net 0.16.11 | 饼干二维码识别 | [许可](docs/licenses/ZXing.Net-LICENSE.txt) |
| Emoji.Wpf 0.3.4 及相关依赖 | Windows 彩色 emoji 绘制 | [说明](docs/licenses/Emoji-NOTICE.txt)、[许可目录](docs/licenses) |
| MiSans | 界面和阅读文字 | [来源](docs/licenses/MiSans-NOTICE.txt)、[字体许可](docs/licenses/MiSans-LICENSE.pdf) |
| Noto Sans Tagbanwa / Batak | 缺字回退 | [Tagbanwa OFL](docs/licenses/NotoSansTagbanwa-OFL.txt)、[Batak OFL](docs/licenses/NotoSansBatak-OFL.txt) |
| Unicode 数据 | emoji 序列识别 | [Unicode 许可](docs/licenses/Unicode-LICENSE.txt) |
| X 岛网页、X岛-EX、蓝岛颜文字资料 | 颜文字菜单 | [资料声明](docs/licenses/EmoticonData-NOTICE.txt)、[源数据](docs/sources/emoticons/README.md) |

字体文件保持原样，未修改字形或安装到系统。Windows 系统字体不随仓库或程序分发。`tools/Build.ps1` 会将适用的许可和声明复制至发布目录的 `licenses/`。
