using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using Tide.Core;

namespace Tide.Desktop;

/// <summary>本机串屏蔽管理。逐行移除保留滚动位置；关闭窗口后解除集合监听。</summary>
public sealed class BlockedThreadsWindow : DialogWindow
{
    public BlockedThreadsWindow(MainViewModel vm) : base("屏蔽管理")
    {
        Width = 580; Height = 520;
        var empty = Hint("暂无屏蔽的串"); Body.Children.Add(empty);
        var rows = new Dictionary<long, FrameworkElement>();
        void Add(BlockedThread thread)
        {
            var panel = new DockPanel { Margin = new(0, 10, 0, 10) };
            var restore = Button("取消屏蔽", () => vm.UnblockThread(thread.Id));
            restore.VerticalAlignment = VerticalAlignment.Center;
            System.Windows.Automation.AutomationProperties.SetName(restore, $"取消屏蔽 No.{thread.Id}");
            DockPanel.SetDock(restore, Dock.Right); panel.Children.Add(restore);
            var label = new StackPanel { Margin = new(0, 0, 10, 0) };
            label.Children.Add(new TextBlock { Text = $"No.{thread.Id}", FontWeight = FontWeights.SemiBold });
            label.Children.Add(new EmojiLabel { Text = ContentText.Plain(thread.Title, vm.State.HideSpoilers), TextTrimming = TextTrimming.CharacterEllipsis, Margin = new(0, 4, 0, 0) });
            panel.Children.Add(label);
            var row = new Border { Child = panel, BorderThickness = new(0, 0, 0, 1) };
            row.SetResourceReference(Border.BorderBrushProperty, "Line"); rows.Add(thread.Id, row); Body.Children.Add(row);
        }
        void RefreshEmpty() => empty.Visibility = rows.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        foreach (var thread in vm.BlockedThreads) Add(thread);
        RefreshEmpty();
        void Changed(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems is not null)
                foreach (BlockedThread thread in e.OldItems)
                    if (rows.Remove(thread.Id, out var row)) Body.Children.Remove(row);
            if (e.NewItems is not null) foreach (BlockedThread thread in e.NewItems) Add(thread);
            RefreshEmpty();
        }
        vm.BlockedThreads.CollectionChanged += Changed;
        Closed += (_, _) => vm.BlockedThreads.CollectionChanged -= Changed;
        AddAction("完成", Close, true);
    }
}
