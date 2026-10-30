using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using Tide.Core;

namespace Tide.Desktop;

/// <summary>通用对话框骨架：固定标题、可滚动内容和固定底部操作栏，统一使用主程序主题。</summary>
public class DialogWindow : ChildWindow
{
    protected StackPanel Body { get; } = new() { Margin = new(26, 8, 26, 20) };
    protected StackPanel Actions { get; } = new() { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new(26, 14, 26, 18) };
    protected TextBlock Feedback { get; } = new() { TextWrapping = TextWrapping.Wrap, Margin = new(0, 12, 0, 8) };
    public DialogWindow(string title, string subtitle = "", double subtitleFontSize = 12) : base(title + " · 潮汐")
    {
        Width = 640; Height = 660; MinWidth = 470; MinHeight = 400;
        var grid = new Grid(); grid.SetResourceReference(BackgroundProperty, "Canvas"); grid.RowDefinitions.Add(new() { Height = GridLength.Auto }); grid.RowDefinitions.Add(new()); grid.RowDefinitions.Add(new() { Height = GridLength.Auto });
        // 没有副标题时不预留说明行，让表单紧接标题。
        var heading = new StackPanel { Margin = new(26, 20, 26, 8) };
        heading.Children.Add(new TextBlock { Text = title, FontSize = 25, FontWeight = FontWeights.SemiBold });
        if (subtitle.Length > 0) { var text = Hint(subtitle); text.FontSize = subtitleFontSize; text.LineHeight = subtitleFontSize * 1.5; heading.Children.Add(text); }
        grid.Children.Add(heading);
        var scroll = new ScrollViewer { Content = Body, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled }; Grid.SetRow(scroll, 1); grid.Children.Add(scroll);
        UseReadingScroll(scroll);
        var footer = new Border { Child = Actions, BorderThickness = new(0, 1, 0, 0) }; footer.SetResourceReference(Border.BorderBrushProperty, "Line"); Grid.SetRow(footer, 2); grid.Children.Add(footer);
        Feedback.SetResourceReference(TextBlock.ForegroundProperty, "Danger");
        SetClientContent(grid);
    }
    protected static TextBlock Hint(string text) { var b = new TextBlock { Text = text, TextWrapping = TextWrapping.Wrap, FontSize = 12, Margin = new(0, 9, 0, 9), LineHeight = 21 }; b.SetResourceReference(TextBlock.ForegroundProperty, "Muted"); return b; }
    protected void Label(string text) => Body.Children.Add(new TextBlock { Text = text, FontWeight = FontWeights.SemiBold, Margin = new(0, 16, 0, 8) });
    protected static Button Button(string label, Action action, bool primary = false)
    {
        var b = new Wpf.Ui.Controls.Button { Content = label, Margin = new(8, 0, 0, 0) };
        if (primary) b.SetResourceReference(StyleProperty, "Primary");
        b.Click += (_, _) => action(); return b;
    }
    public void AddAction(string text, Action action, bool primary = false) => Actions.Children.Add(Button(text, action, primary));
}

/// <summary>复用正文控件展示版规、发送预览或单条引用，调用方可追加操作按钮。</summary>
public sealed class TextWindow : DialogWindow
{
    private readonly PostBody _body = new() { FontSize = 16 };
    public TextWindow(string title, string html) : base(title)
    {
        Body.Children.Add(_body); SetHtml(html); AddAction("关闭", Close);
    }
    public void SetHtml(string html) => _body.Html = html;
    public void SetPost(Post post)
    {
        SetHtml(post.Content);
        Body.Children.Insert(0, new PostIdentity { Post = post, Margin = new(0, 0, 0, 6) });
        var timestamp = Hint($"{PostTime.Format(post.Time, includeSeconds: true)}  ·  {post.Number}");
        timestamp.ToolTip = post.Time;
        Body.Children.Insert(1, timestamp);
        if (post.HasImage) Body.Children.Add(new PostImage { Post = post, Margin = new(0, 20, 0, 0) });
    }
}

/// <summary>外观设置直接更新共享状态并即时保存，无需额外的“应用”步骤。</summary>
public sealed class SettingsWindow : DialogWindow
{
    public SettingsWindow(MainViewModel vm) : base("阅读设置")
    {
        Height = 660;
        Label("正文字号（含串列表预览）");
        var current = new TextBlock { Text = $"{vm.State.FontSize:0} px" };
        var size = new Slider { Minimum = 13, Maximum = 24, TickFrequency = 1, IsSnapToTickEnabled = true, Value = vm.State.FontSize, Margin = new(0, 12, 0, 10) };
        size.ValueChanged += (_, _) => { vm.State.FontSize = size.Value; current.Text = $"{size.Value:0} px"; vm.AppearanceChanged(); };
        Body.Children.Add(current); Body.Children.Add(size);
        var sample = new TextBlock { Text = "正文预览 Aa 123", TextWrapping = TextWrapping.Wrap, Margin = new(0, 10, 0, 10) };
        sample.SetBinding(TextBlock.FontSizeProperty, new System.Windows.Data.Binding(nameof(MainViewModel.ReadingFontSize)) { Source = vm }); Body.Children.Add(sample);
        Label("防剧透");
        var spoilers = new CheckBox { Content = "启用防剧透", IsChecked = vm.State.HideSpoilers, ToolTip = "开启时以黑条遮盖，点击展开；关闭后显示原文及 [h] 标签。" };
        System.Windows.Automation.AutomationProperties.SetName(spoilers, "启用防剧透");
        void SpoilersChanged(object sender, RoutedEventArgs e) { vm.State.HideSpoilers = spoilers.IsChecked == true; vm.AppearanceChanged(); }
        spoilers.Checked += SpoilersChanged; spoilers.Unchecked += SpoilersChanged;
        Body.Children.Add(spoilers);
        Label("滚动");
        var inertia = new CheckBox { Content = "惯性滚动", IsChecked = vm.State.InertialScrolling, ToolTip = "滚轮停止后逐渐减速；关闭时使用普通滚动。跟随 Windows 动画设置。" };
        System.Windows.Automation.AutomationProperties.SetName(inertia, "惯性滚动");
        // Checked/Unchecked 同时覆盖鼠标、键盘与 UI Automation，不能只在 Click 时保存。
        void InertiaChanged(object sender, RoutedEventArgs e) { vm.State.InertialScrolling = inertia.IsChecked == true; vm.AppearanceChanged(); }
        inertia.Checked += InertiaChanged; inertia.Unchecked += InertiaChanged;
        Body.Children.Add(inertia);
        Label("图片与外观");
        var images = new CheckBox { Content = "自动加载正文缩略图", IsChecked = vm.State.ShowImages };
        images.Click += (_, _) => { vm.State.ShowImages = images.IsChecked == true; vm.AppearanceChanged(); }; Body.Children.Add(images);
        var themes = new WrapPanel { Margin = new(0, 14, 0, 0) };
        foreach (var (label, theme) in new[] { ("浅色", "light"), ("深色", "dark") }) themes.Children.Add(Button(label, () => { vm.State.Theme = theme; App.ApplyTheme(theme); vm.AppearanceChanged(); }));
        Body.Children.Add(themes);
        Label("订阅"); Body.Children.Add(Button("管理订阅号", async () => { new SubscriptionWindow(vm) { Owner = this }.ShowDialog(); await vm.LoadSubscriptionsAsync(); }));
        Label("屏蔽"); Body.Children.Add(Button("屏蔽管理", () => new BlockedThreadsWindow(vm) { Owner = this }.ShowDialog()));
        Label("本机数据"); Body.Children.Add(Hint(vm.Store.Root));
        Body.Children.Add(Hint($"历史 {vm.State.History.Count} 个串 · 草稿 {vm.State.Drafts.Count} 份"));
        Label("潮汐 0.2.1");
        Body.Children.Add(Hint("本软件使用 MiSans 字体。"));
        AddAction("打开数据目录", () => Process.Start(new ProcessStartInfo(vm.Store.Root) { UseShellExecute = true })); AddAction("完成", Close, true);
    }
}

/// <summary>列出未发送草稿，通过回调打开编辑器；编辑器关闭后重新读取列表。</summary>
public sealed class DraftWindow : DialogWindow
{
    public DraftWindow(MainViewModel vm, Action<Draft> open) : base("草稿箱")
    {
        void Refresh()
        {
            Body.Children.Clear();
            if (vm.State.Drafts.Count == 0) Body.Children.Add(Hint("暂无草稿"));
            foreach (var draft in vm.State.Drafts.OrderByDescending(d => d.ThreadId))
            {
                var panel = new StackPanel { Margin = new(0, 12, 0, 16) };
                panel.Children.Add(new TextBlock { Text = draft.ThreadId > 0 ? $"回复 No.{draft.ThreadId}" : "新串 · " + vm.ForumName(draft.ForumId), FontWeight = FontWeights.SemiBold });
                var preview = ContentText.Plain(draft.Content, vm.State.HideSpoilers); panel.Children.Add(Hint(preview.Length > 100 ? preview[..100] + "…" : preview));
                var bar = new WrapPanel(); bar.Children.Add(Button("继续编辑", () => { open(draft); Refresh(); }));
                bar.Children.Add(Button("删除草稿", () =>
                {
                    if (MessageBox.Show(this, "删除这份本机草稿？", "删除草稿", MessageBoxButton.OKCancel) != MessageBoxResult.OK) return;
                    vm.State.Drafts.RemoveAll(d => d.Key == draft.Key); vm.Save(); Refresh();
                })); panel.Children.Add(bar); Body.Children.Add(panel);
            }
        }
        Refresh(); AddAction("关闭", Close);
    }
}




