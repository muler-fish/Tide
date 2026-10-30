using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Tide.Core;

namespace Tide.Desktop;

/// <summary>只读列表预览：保留格式与空白，由实际栏宽换行，最多八行；点击交给所在串条目。</summary>
public sealed class PostPreview : TextBlock
{
    static PostPreview() => FontSizeProperty.OverrideMetadata(typeof(PostPreview),
        new FrameworkPropertyMetadata(14d, (d, _) => ((PostPreview)d).UpdateLineLimit()));
    public static readonly DependencyProperty HtmlProperty = DependencyProperty.Register(nameof(Html), typeof(string), typeof(PostPreview),
        new PropertyMetadata("", (d, _) => ((PostPreview)d).Render()));
    public static readonly DependencyProperty HideSpoilersProperty = DependencyProperty.Register(nameof(HideSpoilers), typeof(bool), typeof(PostPreview),
        new PropertyMetadata(false, (d, _) => ((PostPreview)d).Render()));
    public string Html { get => (string)GetValue(HtmlProperty); set => SetValue(HtmlProperty, value); }
    public bool HideSpoilers { get => (bool)GetValue(HideSpoilersProperty); set => SetValue(HideSpoilersProperty, value); }

    public PostPreview()
    {
        // 默认字号已由依赖属性元数据提供；构造时设置局部值会盖过列表模板中的字号绑定。
        FontFamily = AppFonts.Family;
        AppFonts.ApplyReadingRendering(this);
        TextWrapping = TextWrapping.Wrap; TextTrimming = TextTrimming.CharacterEllipsis;
        LineStackingStrategy = LineStackingStrategy.BlockLineHeight;
        // 不另建滚动区，链接和文本点击都继续打开当前串；正文内仍可操作原链接。
        IsHitTestVisible = false; Focusable = false;
        SetResourceReference(ForegroundProperty, "Text");
        SetResourceReference(HideSpoilersProperty, "HideSpoilers");
        UpdateLineLimit();
    }

    private void Render()
    {
        Inlines.Clear();
        var content = ContentText.Normalize(Html);
        if (HideSpoilers) content = Regex.Replace(content, @"\[h\][\s\S]*?\[/h\]", "[隐藏内容]", RegexOptions.IgnoreCase);
        PostBody.AppendHtml(Inlines, content);
        ColorEmoji.Render(Inlines);
        // 文本内有 Hyperlink 也不能形成隐蔽的 Tab 入口，列表项本身保留键盘入口。
        DisableLinkFocus(Inlines);
    }

    private static void DisableLinkFocus(System.Windows.Documents.InlineCollection inlines)
    {
        foreach (var inline in inlines)
        {
            if (inline is System.Windows.Documents.Hyperlink link) link.Focusable = false;
            if (inline is System.Windows.Documents.Span span) DisableLinkFocus(span.Inlines);
        }
    }

    private void UpdateLineLimit() { LineHeight = FontSize * 1.5; MaxHeight = LineHeight * 8; }
}
