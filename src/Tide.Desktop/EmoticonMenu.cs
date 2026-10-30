using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Tide.Desktop;

/// <summary>依附编辑工具栏的下拉菜单：最近使用在上，全部条目在下；点击即插入。</summary>
internal sealed class EmoticonMenu
{
    private readonly Window _owner;
    private readonly Func<IEnumerable<string>> _recent;
    private readonly Action<string> _choose;
    private readonly Action _cancel;
    internal Popup Popup { get; } = new() { Placement = PlacementMode.Bottom, StaysOpen = false, AllowsTransparency = true };

    public EmoticonMenu(Window owner, FrameworkElement anchor, Func<IEnumerable<string>> recent, Action<string> choose, Action cancel)
    {
        _owner = owner; _recent = recent; _choose = choose; _cancel = cancel; Popup.PlacementTarget = anchor;
        owner.Closed += (_, _) => Popup.IsOpen = false;
        owner.LocationChanged += (_, _) => Popup.IsOpen = false;
        owner.SizeChanged += (_, _) => Popup.IsOpen = false;
    }

    public void Toggle()
    {
        if (Popup.IsOpen) { Popup.IsOpen = false; return; }
        var panel = new MenuPanel { Margin = new(8) };
        AutomationProperties.SetName(panel, "颜文字菜单");
        var recent = EmoticonCatalog.Recent(_recent());
        AddHeading(panel, "最近使用");
        if (recent.Count > 0) panel.Children.Add(Buttons(recent));
        else
        {
            var empty = new TextBlock { Text = "暂无", Margin = new(8, 2, 8, 8) }; empty.SetResourceReference(TextBlock.ForegroundProperty, "Muted"); panel.Children.Add(empty);
        }
        var line = new Border { BorderThickness = new(0, 1, 0, 0), Margin = new(4, 8, 4, 8) }; line.SetResourceReference(Border.BorderBrushProperty, "Line"); panel.Children.Add(line);
        AddHeading(panel, "全部颜文字"); panel.Children.Add(Buttons(EmoticonCatalog.Entries));
        var scroll = new ScrollViewer {
            Content = panel, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            MaxHeight = Math.Clamp(_owner.ActualHeight - 100, 180, 480), Width = Math.Clamp(_owner.ActualWidth - 40, 320, 680)
        };
        var border = new Border { Child = scroll, BorderThickness = new(1), CornerRadius = new(4) };
        System.Windows.Documents.TextElement.SetFontFamily(border, AppFonts.Family);
        border.SetResourceReference(Border.BackgroundProperty, "Surface"); border.SetResourceReference(Border.BorderBrushProperty, "Line");
        KeyboardNavigation.SetTabNavigation(panel, KeyboardNavigationMode.Cycle);
        panel.PreviewKeyDown += (_, e) => {
            if (e.Key == Key.Escape) { Popup.IsOpen = false; _cancel(); e.Handled = true; }
            else if (e.Key is Key.Up or Key.Down or Key.Left or Key.Right && Keyboard.FocusedElement is UIElement focused)
            {
                var direction = e.Key switch { Key.Up => FocusNavigationDirection.Up, Key.Down => FocusNavigationDirection.Down, Key.Left => FocusNavigationDirection.Left, _ => FocusNavigationDirection.Right };
                focused.MoveFocus(new TraversalRequest(direction)); e.Handled = true;
            }
        };
        Popup.Child = border; Popup.IsOpen = true;
        panel.Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() => {
            if (Popup.IsOpen) panel.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
        }));
    }
    private WrapPanel Buttons(IEnumerable<EmoticonEntry> entries)
    {
        var wrap = new WrapPanel();
        foreach (var entry in entries)
        {
            var label = new EmojiLabel { Text = entry.Display, FontFamily = AppFonts.Family, FontSize = 15, TextTrimming = TextTrimming.CharacterEllipsis };
            var button = new Button { Content = label, MinWidth = 98, MaxWidth = 220, Height = 36, Margin = new(2), Padding = new(8, 5, 8, 5), Tag = entry };
            button.SetResourceReference(FrameworkElement.StyleProperty, "Quiet");
            AutomationProperties.SetName(button, entry.Display);
            var preview = new EmojiLabel { Text = entry.Text, FontFamily = AppFonts.Family, FontSize = 16, LineHeight = 19, TextWrapping = TextWrapping.NoWrap };
            button.ToolTip = new ToolTip { Content = new ScrollViewer { Content = preview, MaxWidth = 540, MaxHeight = 340, HorizontalScrollBarVisibility = ScrollBarVisibility.Auto, VerticalScrollBarVisibility = ScrollBarVisibility.Auto } };
            button.Click += (_, _) => { Popup.IsOpen = false; _choose(entry.Text); };
            wrap.Children.Add(button);
        }
        return wrap;
    }
    private static void AddHeading(Panel panel, string title)
    {
        var heading = new TextBlock { Text = title, FontWeight = FontWeights.SemiBold, Margin = new(8, 4, 8, 8), FontSize = 13 };
        heading.SetResourceReference(TextBlock.ForegroundProperty, "Muted"); panel.Children.Add(heading);
    }
    private sealed class MenuPanel : StackPanel
    {
        protected override AutomationPeer OnCreateAutomationPeer() => new FrameworkElementAutomationPeer(this);
    }
}
