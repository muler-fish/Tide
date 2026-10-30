using System.ComponentModel;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.Win32;
using Tide.Core;

namespace Tide.Desktop;

/// <summary>新串与回复共用的编辑器，负责草稿、附件选择、发送预览和人工确认后的提交。</summary>
public sealed class ComposerWindow : DialogWindow
{
    private readonly MainViewModel _vm;
    private readonly long _threadId;
    private readonly string _key;
    private readonly ComboBox _forum = new() { DisplayMemberPath = "Name", MinHeight = 36, Padding = new(8) };
    private readonly ComboBox _cookie = new() { DisplayMemberPath = "Name", MinHeight = 36, Padding = new(8) };
    private readonly Wpf.Ui.Controls.TextBox _title = new() { MaxLength = 100 };
    private readonly Wpf.Ui.Controls.TextBox _name = new() { MaxLength = 100 };
    private readonly EmojiEditor _content = new() { MinHeight = 220, MaxHeight = 380, VerticalContentAlignment = VerticalAlignment.Top, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, MaxLength = 10000, FontSize = 16 };
    private readonly CheckBox _water = new() { Content = "为附图添加水印" };
    private readonly TextBlock _imageName = new() { Text = "未选择图片", VerticalAlignment = VerticalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis, MaxWidth = 270, Margin = new(10, 0, 0, 0) };
    private readonly TextBlock _count = new() { HorizontalAlignment = HorizontalAlignment.Right, Margin = new(0, 6, 0, 0), FontSize = 11 };
    // 用户停止输入 600 毫秒后再保存，避免每次按键都把整个状态文件写入磁盘。
    private readonly DispatcherTimer _autosave = new() { Interval = TimeSpan.FromMilliseconds(600) };
    private string _imageFile = "";
    private bool _sending, _initialized;
    private int _insertStart, _insertLength;
    internal EmoticonMenu Emoticons { get; }
    public bool Delivered { get; private set; }

    public ComposerWindow(MainViewModel vm, long threadId, string quote = "", Draft? existing = null)
        : base(threadId > 0 ? "参与回应" : "发一个串", threadId > 0 ? $"No.{threadId}" : "", 20)
    {
        Width = 720; Height = 850; _vm = vm; _threadId = threadId;
        System.Windows.Automation.AutomationProperties.SetName(_content, "发言正文");
        System.Windows.Automation.AutomationProperties.SetName(_title, "可选标题");
        System.Windows.Automation.AutomationProperties.SetName(_name, "可选名称");
        _content.FontFamily = AppFonts.Family;
        System.Windows.Automation.AutomationProperties.SetName(_forum, "发串版块");
        System.Windows.Automation.AutomationProperties.SetName(_cookie, "发言使用的饼干");
        var defaultForum = vm.ActiveForum is { IsTimeline: false } active ? active.Id : 4;
        // 草稿键在本次编辑期间保持不变，切换发串版块时更新同一草稿，不额外留下重复项。
        _key = existing?.Key ?? (threadId > 0 ? $"reply:{threadId}" : $"new:{defaultForum}");
        existing ??= vm.State.Drafts.Find(d => d.Key == _key);
        _forum.ItemsSource = vm.State.Forums.Where(f => !f.IsTimeline).ToList();
        _forum.SelectedItem = vm.State.Forums.Find(f => !f.IsTimeline && f.Id == (existing?.ForumId ?? defaultForum));
        if (threadId == 0) { Label("发到哪个版块"); Body.Children.Add(_forum); }
        Label("使用的饼干");
        var identityBar = new DockPanel(); var manage = Button("管理饼干", () =>
        {
            new CookieWindow(vm) { Owner = this }.ShowDialog(); RefreshCookies();
        }); DockPanel.SetDock(manage, Dock.Right); identityBar.Children.Add(manage); identityBar.Children.Add(_cookie); Body.Children.Add(identityBar); RefreshCookies();
        var optional = new StackPanel { Margin = new(8, 4, 0, 8) };
        optional.Children.Add(new TextBlock { Text = "标题", Margin = new(0, 4, 0, 6) }); optional.Children.Add(_title);
        optional.Children.Add(new TextBlock { Text = "名称", Margin = new(0, 12, 0, 6) }); optional.Children.Add(_name);
        var options = new Expander { Header = "可选项", Content = optional, IsExpanded = false, Margin = new(0, 12, 0, 0) };
        System.Windows.Automation.AutomationProperties.SetName(options, "可选项"); Body.Children.Add(options);
        Label("正文"); Body.Children.Add(_content); _count.SetResourceReference(TextBlock.ForegroundProperty, "Muted"); Body.Children.Add(_count);
        var tools = new WrapPanel { Margin = new(0, 6, 0, 8) };
        var emoticons = Button("颜文字", OpenEmoticons); tools.Children.Add(emoticons);
        Emoticons = new EmoticonMenu(this, emoticons, () => _vm.State.RecentEmoticons, text => {
            RestoreInsertSelection();
            if (Insert(text)) { _vm.State.RecentEmoticons = EmoticonCatalog.Remember(_vm.State.RecentEmoticons, text); _vm.Save(); }
        }, RestoreInsertSelection);
        tools.Children.Add(Button("防剧透", () => { var selected = _content.SelectedText; _content.SelectedText = "[h]" + selected + "[/h]"; _content.Focus(); }));
        tools.Children.Add(Button("骰子", () => Insert("[1,100]"))); Body.Children.Add(tools);
        Label("附图（JPG / PNG / GIF，最多 5 MB）");
        var imageBar = new WrapPanel(); imageBar.Children.Add(Button("选择图片", PickImage)); imageBar.Children.Add(_imageName); imageBar.Children.Add(Button("移除", () => { _imageFile = ""; _imageName.Text = "未选择图片"; SaveDraft(); })); Body.Children.Add(imageBar); Body.Children.Add(_water);
        Body.Children.Add(Feedback);
        AddAction("保存并关闭", Close); AddAction("预览并发送", Preview, true);
        if (existing is not null)
        {
            _title.Text = existing.Title; _name.Text = existing.Name ?? ""; _content.Text = existing.Content; _imageFile = existing.ImageFile; _water.IsChecked = existing.Watermark;
            _imageName.Text = string.IsNullOrEmpty(_imageFile) ? "未选择图片" : Path.GetFileName(_imageFile);
        }
        if (quote.Length > 0) _content.Text = quote + _content.Text;
        // 先恢复草稿，再监听输入变化，避免初始化控件时保存一份尚未恢复完整的内容。
        _initialized = true; UpdateCount();
        _content.TextChanged += (_, _) => { UpdateCount(); ScheduleSave(); }; _title.TextChanged += (_, _) => ScheduleSave();
        _name.TextChanged += (_, _) => ScheduleSave();
        _forum.SelectionChanged += (_, _) => ScheduleSave(); _water.Click += (_, _) => ScheduleSave();
        _autosave.Tick += (_, _) => { _autosave.Stop(); SaveDraft(); };
        Closing += OnClosing; Closed += (_, _) => _autosave.Stop();
    }
    private void RefreshCookies()
    {
        _cookie.ItemsSource = _vm.State.Cookies.ToList(); _cookie.SelectedItem = _vm.State.Cookies.Find(c => c.Id == _vm.State.ActiveCookieId);
    }
    private void OpenEmoticons()
    {
        _insertStart = _content.SelectionStart; _insertLength = _content.SelectionLength; Emoticons.Toggle();
    }
    private void RestoreInsertSelection() { _content.Focus(); _content.Select(_insertStart, _insertLength); }
    private bool Insert(string text)
    {
        // 一次插入一个完整图案；超长时不让 TextBox 截断图案或 Unicode 组合。
        text = text.Replace("\r\n", "\n").Replace("\n", Environment.NewLine);
        if (_content.Text.Length - _content.SelectionLength + text.Length > _content.MaxLength)
        { Feedback.Text = "正文最多 10000 字，剩余空间不足以插入完整内容。"; _content.Focus(); return false; }
        var start = _content.SelectionStart;
        _content.BeginChange();
        try { _content.SelectedText = text; _content.Select(start + text.Length, 0); }
        finally { _content.EndChange(); }
        Feedback.Text = ""; _content.Focus(); return true;
    }
    private void UpdateCount() => _count.Text = $"{_content.Text.Length} / 10000";
    private void ScheduleSave() { _autosave.Stop(); _autosave.Start(); }
    private void SaveDraft()
    {
        // 已确认送达后不能在 Closing 或定时器里重新写回草稿。
        if (!_initialized || Delivered) return;
        var draft = new Draft(_key, (_forum.SelectedItem as Forum)?.Id ?? 4, _threadId, _title.Text, _content.Text, _imageFile, _water.IsChecked == true, _name.Text);
        _vm.State.Drafts.RemoveAll(d => d.Key == _key);
        if (!string.IsNullOrWhiteSpace(draft.Content) || draft.Title.Length > 0 || draft.Name.Length > 0 || draft.ImageFile.Length > 0) _vm.State.Drafts.Add(draft);
        _vm.Save();
    }
    private void OnClosing(object? sender, CancelEventArgs e)
    {
        // 发送过程中保持窗口存在以接收结果；取消等待并不能撤回已经到达服务端的发言。
        if (_sending) { Feedback.Text = "正在发送，请等待返回结果后再关闭。"; e.Cancel = true; return; }
        SaveDraft();
    }
    private void PickImage()
    {
        var dialog = new OpenFileDialog { Filter = "支持的图片|*.jpg;*.jpeg;*.png;*.gif", Multiselect = false };
        if (dialog.ShowDialog(this) != true) return;
        try
        {
            var file = new FileInfo(dialog.FileName);
            if (file.Length > 5 * 1024 * 1024) throw new ArgumentException("图片不能超过 5 MB。");
            // 选择时先检查文件头；发送前 API 层还会重新读取，防止文件在编辑期间被替换。
            using var stream = file.OpenRead(); var bytes = new byte[8]; var n = stream.Read(bytes);
            if (XIslandApi.DetectImage(bytes.AsSpan(0, n)) is null) throw new ArgumentException("图片格式不正确，请使用 JPG、PNG 或 GIF。");
            _imageFile = file.FullName; _imageName.Text = file.Name; Feedback.Text = ""; SaveDraft();
        }
        catch (Exception ex) { Feedback.Text = ex.Message; }
    }
    private void Preview()
    {
        if (_sending) return;
        if (string.IsNullOrWhiteSpace(_content.Text)) { Feedback.Text = "先写一点正文吧。"; _content.Focus(); return; }
        if (_cookie.SelectedItem is not CookieIdentity cookie) { Feedback.Text = "请先导入并选择一份饼干。"; return; }
        if (_threadId == 0 && _forum.SelectedItem is not Forum) { Feedback.Text = "请选择一个发串版块。"; return; }
        SaveDraft();
        // 确认按钮闭包捕获这份不可变请求和选中饼干，不再读取之后可能变化的界面字段。
        var post = new PostRequest((_forum.SelectedItem as Forum)?.Id ?? 4, _threadId, _title.Text, _content.Text, _imageFile, _water.IsChecked == true, _name.Text);
        var destination = _threadId > 0 ? $"回复 No.{_threadId}" : _vm.ForumName(post.ForumId);
        // 用户输入先 HTML 编码，再交给统一正文控件预览，避免把输入内容误当作预览模板标签。
        var html = $"<b>{WebUtility.HtmlEncode(destination)} · 饼干：{WebUtility.HtmlEncode(cookie.Name)}</b><br><br>" +
            (post.Title.Length > 0 ? $"<b>标题：{WebUtility.HtmlEncode(post.Title)}</b><br>" : "") +
            (post.Name.Length > 0 ? $"<b>名称：{WebUtility.HtmlEncode(post.Name)}</b><br>" : "") + "<br>" + WebUtility.HtmlEncode(post.Content).Replace("\n", "<br>");
        if (_imageFile.Length > 0) html += "<br><br>附图：" + WebUtility.HtmlEncode(Path.GetFileName(_imageFile));
        var preview = new TextWindow("发送预览", html) { Owner = this };
        preview.AddAction("确认发送", async () =>
        {
            if (_sending) return;
            preview.Close(); _sending = true; Body.IsEnabled = false; Actions.IsEnabled = false; Feedback.Text = "正在发送，请稍候…";
            try
            {
                var secret = SecretProtection.Unprotect(cookie.EncryptedValue);
                var result = await _vm.Api.SendPostAsync(post, secret);
                // 只有接口明确确认成功才删除草稿；异常（包括送达结果未知）会保留编辑内容。
                Delivered = true; _vm.State.Drafts.RemoveAll(d => d.Key == _key); _vm.Save(); _vm.Status = result.Message;
                _sending = false; Close();
            }
            catch (Exception ex) { Feedback.Text = ex is System.Net.Http.HttpRequestException ? "建立发言会话失败，尚未提交。请检查网络后重试。" : ex.Message; }
            finally { _sending = false; Body.IsEnabled = true; Actions.IsEnabled = true; }
        }, true);
        preview.ShowDialog();
    }
}
