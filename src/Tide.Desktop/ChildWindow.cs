using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Wpf.Ui.Controls;

namespace Tide.Desktop;

/// <summary>应用子窗口共用的 Fluent 外壳：统一标题栏、主题、关闭入口和首次显示尺寸。</summary>
public abstract class ChildWindow : FluentWindow
{
    private readonly ContentPresenter _client = new();

    protected ChildWindow(string title)
    {
        Title = title;
        // 显式沿用 FluentWindow 模板，不能再引用旧 Window 样式，否则会退回系统标题栏。
        SetResourceReference(StyleProperty, typeof(FluentWindow));
        ExtendsContentIntoTitleBar = true;
        WindowBackdropType = WindowBackdropType.None;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        WindowPlacement.FitOnStartup(this);

        var caption = new TitleBar { Height = 44, Padding = new(16, 0, 0, 0) };
        caption.SetBinding(TitleBar.TitleProperty, new Binding(nameof(Title)) { Source = this });
        caption.SetResourceReference(BackgroundProperty, "Sidebar");
        caption.SetResourceReference(ForegroundProperty, "Text");
        caption.SetResourceReference(TitleBar.ButtonsForegroundProperty, "Text");
        System.Windows.Automation.AutomationProperties.SetName(caption, "窗口标题栏");
        var buttons = new Style(typeof(TitleBarButton), (Style)FindResource(typeof(TitleBarButton)));
        buttons.Setters.Add(new Setter(HeightProperty, 44d));
        buttons.Setters.Add(new Setter(WidthProperty, 44d));
        caption.Resources.Add(typeof(TitleBarButton), buttons);

        var shell = new Grid();
        shell.SetResourceReference(Panel.BackgroundProperty, "Canvas");
        shell.RowDefinitions.Add(new() { Height = new GridLength(44) });
        shell.RowDefinitions.Add(new());
        var divider = new Border { Child = caption, BorderThickness = new(0, 0, 0, 1) };
        divider.SetResourceReference(Border.BorderBrushProperty, "Line");
        shell.Children.Add(divider);
        Grid.SetRow(_client, 1); shell.Children.Add(_client);
        Content = shell;

        // Esc 和标题栏关闭都走正常 Window.Close；编辑器仍能在 Closing 中保存草稿或取消关闭。
        PreviewKeyDown += (_, e) => { if (e.Key == Key.Escape) { Close(); e.Handled = true; } };
    }

    // 派生窗口只提供客户区内容，避免无意替换掉公共标题栏。
    protected void SetClientContent(UIElement content) => _client.Content = content;

    /// <summary>子窗口的内容滚动跟随所属主窗口设置；输入框内部和下拉框仍用原生滚动。</summary>
    protected void UseReadingScroll(FrameworkElement surface)
    {
        Loaded += (_, _) =>
        {
            Window? owner = Owner;
            while (owner is not null && owner is not MainWindow) owner = owner.Owner;
            if (owner is MainWindow main)
                surface.SetBinding(InertialScroll.IsEnabledProperty, new Binding(nameof(MainViewModel.InertialScrolling)) { Source = main.Model });
        };
    }
}
