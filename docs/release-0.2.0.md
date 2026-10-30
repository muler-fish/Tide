# 潮汐 0.2.0 正式版

交付日期：2026-10-30。下载：[GitHub Release v0.2.0](https://github.com/muler-fish/Tide/releases/tag/v0.2.0)；本机产物为 `release/0.2.0/Tide.exe`。Windows x64 自包含版本，无需单独安装 .NET。按用户要求去掉预览标记，程序元数据、主窗口和阅读设置中的版本均为 0.2.0。

第 10–12 项已获用户确认并移入 [归档](todo-archive.md)。本轮第 13、14 项已实现，仍在 [TODO](../TODO.txt) 中等待联合验收。正式版名称由用户指定，不代替这两项的视觉验收。旧版及所有预览版保留。

## 第 13 项：点击后残留底色

两个原因分别处理：串列表的 `IsSelected` 触发器让已打开项一直保持绿色；普通导航按钮将 `IsKeyboardFocused` 和悬停映射为同一个底色，鼠标离开后焦点还在，因此仍像悬停。

- 串列表使用独立的 `ReadingListItem` 模板，只在鼠标悬停时填充背景。保留选择和打开串的逻辑，不用清空选中项来消除颜色。
- `Quiet` 普通按钮取消焦点对应的背景触发器，继续提供悬停、按下、禁用反馈和键盘焦点环。
- 饼干、订阅号等管理列表继续使用原选中样式；PO、主题、收藏等真正的状态控件不受影响。串阅读区原本就是透明项模板，本轮保持原样。

## 第 14 项：正文抗锯齿

使用用户截图中的文字和相同字号，分别比较 Display / Ideal 排版、Auto / ClearType / Grayscale 抗锯齿、Fixed / Animated 字形提示，并比较用户提供的 MiSans TTF 和 OTF。正文原先已实际采用 Ideal；只切换排版或改用 OTF 改善不明显。

本机 125% 显示缩放下，Auto 与 ClearType 有彩色边缘；Grayscale / Fixed 更干净，Grayscale / Animated 则更模糊。因此保留现有 MiSans 字库，给 `PostPreview` 和 `PostBody`（含内部 FlowDocument）明确设置 Ideal + Grayscale + Fixed，由 `AppFonts.ApplyReadingRendering` 统一维护。引用、串预览小窗及其他复用这些控件的正文也沿用该设置。

这次没有替换字库、增加字体包、修改系统 ClearType 设置或模糊文字位图。字号、行距、八行预览上限和字形回退不变。Windows 彩色 emoji 仍独立绘制。小字号界面标签保持原渲染方式；其他显示器和缩放比例的观感尚未验证，最终以用户实际阅读感受验收。

## 技能与验证

本轮使用 `ui-ux-pro-max` 的状态区分、键盘焦点和文字可读性原则，以及 `diagnosing-bugs` 的复现、对照、修复与局部回归流程。

修改前，`--reading-polish` 已复现失败：鼠标离开后串预览背景为 `#FFDCEDE5`，按钮背景为 `#FFD8E3E0`。修改后均为透明 `#00FFFFFF`，真实点击仍打开目标串，键盘焦点保留。

仅运行本次修改相关的专项：

```powershell
dotnet build Tide.slnx -c Release --no-restore --nologo
.\tests\Tide.Desktop.Tests\bin\Release\net10.0-windows\Tide.Desktop.Tests.exe --reading-polish artifacts/reading-polish-0.2.0-final
```

- Release 编译 0 警告、0 错误；专项退出码 0，stderr 为空。
- 固定 HTTP 返回和隔离资料目录，真实鼠标点击打开串，再移出；检查深浅主题悬停与移出、透明选中项、键盘焦点样式、PO 开关状态、MiSans 渲染、八行上限、文字选区和 emoji 原文及独立绘制。
- 已查看本机 120 DPI（125%）的深浅主题原生窗口截图。对照中使用实际屏幕像素，因为 WPF 的 `RenderTargetBitmap` 截图不能准确反映屏幕上的 ClearType 色边。
- 首次修复后检查因测试错误地寻找串阅读区不存在的 `Item` 边框失败；改为检查实际 `PostRow`，未为测试改动正文模板。仅重跑同一专项后通过。
- 没有运行全量核心测试、完整 EXE 巡检或无关图片、链接、订阅专项；没有使用真实饼干或执行网络写入。正式包成功生成并核对元数据，本轮未额外启动发布 EXE。

诊断对照：`artifacts/font-native-comparison/`；最终记录：`artifacts/reading-polish-0.2.0-final/`。字体对照命令 `--font-comparison` 仅用于诊断，不是每次构建要求。试验 OTF 位于 `artifacts/font-diagnostics/`，不参与编译和发布。

## 文件大小

| 项目 | 字节 |
| --- | ---: |
| Tide.exe | 79,489,026 |
| 发布目录（含 15 份许可/声明及校验清单） | 79,639,713 |
| 运行时提取目录估算 | 8,215,480 |
| 应用文件总占用估算 | 87,855,193 |

发布目录 **79.64 MB**；含运行时提取的估算 **87.86 MB**，低于十进制 100 MB。运行时估算沿用 preview.30 的实测值，本轮依赖未变，未重新启动 EXE 测量。个人资料、缓存、下载图片、旧版和开发目录不计入该合计。

SHA-256：`8E7A4E0EFDAB5F5705B0F3871FC76464FA0DA7D202CEA22DF041FDEC7A7E6449`。

## 联合验收

1. 打开串后将鼠标移出串列表项，再点击左侧普通导航按钮并移开：底色应恢复；再次悬停仍有反馈。PO 开关开启后仍有状态提示。
2. 用平常的显示缩放及阅读字号查看串列表、串内正文，重点看截图中的“三千……先生。”和“栖霞区”等文字，确认边缘及整体阅读感受。

通过后再归档第 13、14 项。
