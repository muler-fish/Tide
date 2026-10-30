# 0.2.0-preview.4 · 导航与列表精简

日期：2026-10-22。依据用户对 preview.3 的截图批注实施。

## 需求与设计

- 将“发现岛屿”改为“综合线”，保留固定的首页快捷入口。此入口原本就打开综合时间线，现在名称与实际行为一致；从收藏或历史列表也可直接返回。
- 删除列表标题下的描述句和“在当前列表中查找”输入框。保留标题、图标刷新按钮、页码 / 串数与版规，串列表随之上移。同步移除已无入口的本地过滤状态和副标题属性。
- 顶部串号 / 链接跳转继续保留。空列表提示改为“刷新列表，或选择其他版块”，避免指向已经更名的“发现页”。
- 版块分组标题的按钮横向拉伸到与下方整行相同的宽度，高度至少 36 DIP。文字右侧空白、箭头与文字均属于同一个展开按钮，右边界仍避开滚动条。
- 分组仍默认收起，保留浅深色悬停 / 按下反馈、键盘焦点框和空格键切换。

## 技能与实现

继续使用 `ui-ux-pro-max`，应用其点击目标、键盘可达、状态反馈和减少冗余内容的指导；针对现有 WPF 界面查询 `hit target keyboard focus --stack wpf`。本轮沿用既有颜色、图标及控件体系。

分组模板中的 `HeaderSite` 原先未显式设置横向对齐，在当前样式下点击区域收缩到内容宽度。修正 [App.xaml](../src/Tide.Desktop/App.xaml) 中 Expander 与标题按钮的 Stretch 对齐，因此拖动分栏和缩放窗口时仍能跟随布局。

相关源码：[MainWindow.xaml](../src/Tide.Desktop/MainWindow.xaml)、[MainWindow.xaml.cs](../src/Tide.Desktop/MainWindow.xaml.cs)、[MainViewModel.cs](../src/Tide.Desktop/MainViewModel.cs)。

## 验证与交付

在现有 `ForumNavigationChecks` 中增加实际命中与边界检查：标题右侧空白命中展开按钮，标题和版块行左右边界一致，展开 / 收起与键盘焦点有效；从收藏和历史点击“综合线”均返回时间线。沿用浅深色、窄侧栏、收藏持久化和刷新检查。

验证已通过：Release 编译无警告和错误、18 项核心测试、完整源码界面回归与最终 EXE 窗口检查均通过。148 DIP 侧栏中的分组标题与版块行宽度均为 117.6 DIP，右侧距滚动条 3.2 DIP；900 / 1060 / 1280 DIP 窗口下均成立。

截图：[带内容的列表](../artifacts/ui-qa-0.2.0-preview4/groups-expanded.png)、[浅色](../artifacts/ui-qa-0.2.0-preview4/sidebar-favorites-light.png)、[深色](../artifacts/ui-qa-0.2.0-preview4/sidebar-favorites-dark.png)。源码截图使用固定测试内容，不是实时论坛内容；[最终 EXE 截图](../artifacts/window-published-0.2.0-preview4/favorite-removed.png)使用隔离资料。

验收结果和最终 EXE 校验值记录于 [verification.md](verification.md)。新版交付到 `release/0.2.0-preview.4/Tide.exe`，保留 preview.3。默认沿用原资料目录，运行前先退出旧版；独立试用可使用 `--data-dir` 参数。
