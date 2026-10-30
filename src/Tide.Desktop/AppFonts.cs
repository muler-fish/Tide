using System.Windows;
using System.Windows.Media;

namespace Tide.Desktop;

/// <summary>全应用共用的文字字体；图标仍使用 WPF UI 自己的图标字体。</summary>
public static class AppFonts
{
    // 由复合字体优先选用随包 MiSans，缺失字符再回退；不安装或修改系统字体。
    public static FontFamily Family { get; } = new(
        new Uri("pack://application:,,,/"),
        "/Tide;component/Assets/Fonts/#Tide Reading");

    // 阅读文字保留 MiSans 的自然字距，以灰度抗锯齿避免非整数缩放下的彩色边缘。
    // Fixed 保留静态文字的清晰度；不对文本做位图缩放或模糊，也不影响独立绘制的彩色 emoji。
    internal static void ApplyReadingRendering(DependencyObject target)
    {
        TextOptions.SetTextFormattingMode(target, TextFormattingMode.Ideal);
        TextOptions.SetTextRenderingMode(target, TextRenderingMode.Grayscale);
        TextOptions.SetTextHintingMode(target, TextHintingMode.Fixed);
    }
}
