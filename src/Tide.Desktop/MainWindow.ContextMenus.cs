using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Tide.Core;

namespace Tide.Desktop;

public partial class MainWindow
{
    // 阻止 ListBoxItem 默认的右键选中行为；鼠标抬起仍由 WPF 打开原生菜单。
    // 否则右键想屏蔽一个未打开的串，也会触发 SelectionChanged 并打断当前阅读。
    private void Thread_RightButtonDown(object sender, MouseButtonEventArgs e) => e.Handled = true;

    private static void MenuAction(ContextMenu menu, string title, Action action, bool enabled = true)
    {
        var item = new MenuItem { Header = title, IsEnabled = enabled };
        item.Click += (_, _) => action(); menu.Items.Add(item);
    }

    private void ThreadMenu_Opened(object sender, RoutedEventArgs e)
    {
        var menu = (ContextMenu)sender; menu.Items.Clear();
        if (menu.PlacementTarget is FrameworkElement { DataContext: PostCard card } target)
            MenuAction(menu, "屏蔽此串", () => RunFeedbackAction(FeedbackArea.List, target, () => Model.BlockThread(card.Post)));
    }

    private void ForumMenu_Opened(object sender, RoutedEventArgs e)
    {
        var menu = (ContextMenu)sender; menu.Items.Clear();
        if (menu.PlacementTarget is FrameworkElement { DataContext: Forum forum } target)
            MenuAction(menu, Model.FavoriteForumKeys.Contains(forum.Key) ? "取消收藏板块" : "收藏板块",
                () => RunFeedbackAction(FeedbackArea.Navigation, target, () => Model.ToggleForumFavorite(forum)));
    }

    private void PostMenu_Opened(object sender, RoutedEventArgs e)
    {
        var menu = (ContextMenu)sender; menu.Items.Clear();
        if (menu.PlacementTarget is not FrameworkElement { DataContext: ReadPost entry } target) return;
        var body = target as PostBody ?? ReadingAnchor.Children<PostBody>(target).FirstOrDefault();
        // 从已经渲染的文档复制，保留换行和彩色 emoji，且不会泄漏未展开的防剧透文字。
        // 菜单打开时保存选区，避免键盘焦点切到菜单后丢失选中的内容。
        var text = body is null ? "" : !body.Selection.IsEmpty
            ? EmojiText.Read(body.Selection.Start, body.Selection.End)
            : EmojiText.Read(body.Document.ContentStart, body.Document.ContentEnd).TrimEnd('\r', '\n');
        MenuAction(menu, "复制", () =>
        {
            try { Clipboard.SetText(text); ShowFeedback("已复制", FeedbackArea.Reader, anchor: target); }
            catch (ExternalException) { ShowFeedback("剪贴板暂时不可用，请重试。", FeedbackArea.Reader, true, target); }
        }, text.Length > 0);
        // Tips 不是发言，不能引用；线程号在打开菜单时取快照，避免异步切串改变回应目标。
        if (!entry.IsTip && Model.Original is { } original)
            MenuAction(menu, "回应", () => OpenComposer(original.Id, $">>No.{entry.Post.Id}\n"));
    }
}
