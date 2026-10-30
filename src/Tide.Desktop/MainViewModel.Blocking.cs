using System.Collections.ObjectModel;
using System.Windows.Data;
using Tide.Core;

namespace Tide.Desktop;

public sealed partial class MainViewModel
{
    private readonly HashSet<long> _blockedThreadIds = [];
    public ObservableCollection<BlockedThread> BlockedThreads { get; } = [];
    public ListCollectionView VisiblePosts { get; }
    public int VisiblePostCount => VisiblePosts.Count;

    // 只过滤显示视图。原始 Posts 仍参与去重、续页和订阅同步，避免整页被屏蔽后误判已到底。
    private void InitializeBlocking()
    {
        foreach (var thread in State.BlockedThreads)
        {
            _blockedThreadIds.Add(thread.Id); BlockedThreads.Add(thread);
        }
        VisiblePosts.Filter = item => item is PostCard card && !_blockedThreadIds.Contains(card.Post.Id);
        ((System.Collections.Specialized.INotifyCollectionChanged)VisiblePosts).CollectionChanged += (_, _) =>
        {
            Changed(nameof(EmptyList)); Changed(nameof(ListPageLabel)); Changed(nameof(SubscriptionListLabel));
        };
    }

    public void BlockThread(Post post)
    {
        if (post.Id <= 0 || post.IsSiteTip || !_blockedThreadIds.Add(post.Id)) return;
        BlockedThreads.Add(new(post.Id, post.DisplayTitle));
        if (SaveBlockedThreads()) Notify("已屏蔽此串", FeedbackArea.List);
    }

    public void UnblockThread(long id)
    {
        if (!_blockedThreadIds.Remove(id)) return;
        var entry = BlockedThreads.First(t => t.Id == id);
        BlockedThreads.Remove(entry); SaveBlockedThreads();
    }

    private bool SaveBlockedThreads()
    {
        State.BlockedThreads = BlockedThreads.ToList();
        // 解除屏蔽直接显示当前已加载的项目，不重新请求网络，也不重置正文阅读位置。
        VisiblePosts.Refresh();
        try { Store.Save(State); return true; }
        catch (Exception ex) when (ex is System.IO.IOException or UnauthorizedAccessException)
        {
            SetError("本次屏蔽更改已生效，但无法保存；请检查数据目录是否可写。", FeedbackArea.List); return false;
        }
    }
}
