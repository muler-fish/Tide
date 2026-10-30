namespace Tide.Core;

/// <summary>主窗口布局使用 DIP 保存，不与某个屏幕的物理像素或位置绑定。</summary>
public sealed class WindowLayoutState
{
    public double Width { get; set; } = 1280;
    public double Height { get; set; } = 800;
    public bool Maximized { get; set; }
    public double NavigationWidth { get; set; } = 184;
    public double ListWidth { get; set; } = 320;

    public void Normalize()
    {
        Width = Valid(Width, 1280, 320, 16384);
        Height = Valid(Height, 800, 240, 16384);
        NavigationWidth = Valid(NavigationWidth, 184, 148, 260);
        ListWidth = Valid(ListWidth, 320, 240, 16384);
    }

    private static double Valid(double value, double fallback, double min, double max) =>
        double.IsFinite(value) && value > 0 ? Math.Clamp(value, min, max) : fallback;
}
