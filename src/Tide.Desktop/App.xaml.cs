using System.IO;
using System.Windows;
using System.Windows.Media;
using Tide.Core;

namespace Tide.Desktop;

/// <summary>程序入口：确定资料目录、限制重复实例、组装服务并创建主窗口。</summary>
public partial class App : Application
{
    private Mutex? _instance;
    private bool _ownsInstance;
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        string? Value(string flag) { var i = Array.IndexOf(e.Args, flag); return i >= 0 && i + 1 < e.Args.Length ? e.Args[i + 1] : null; }
        // 显式指定的数据目录优先；验证模式默认使用输出目录下的 profile，日常运行放在 LocalAppData。
        var smoke = Value("--smoke-dir");
        var root = Value("--data-dir") ?? (smoke is not null ? Path.Combine(Path.GetFullPath(smoke), "profile") : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TideIsland"));
        try
        {
            // 用完整资料路径生成互斥名称：同一目录只能有一个实例，不同资料目录可分别运行。
            var key = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(Path.GetFullPath(root).ToUpperInvariant())))[..24];
            _instance = new Mutex(true, "Local\\TideIsland-" + key, out _ownsInstance);
            if (!_ownsInstance)
            {
                MessageBox.Show("这个资料目录的潮汐已经在运行，请从任务栏打开。", "潮汐"); Shutdown(); return;
            }
            var vm = new MainViewModel(new XIslandApi(), new LocalStore(root));
            // 先应用已保存主题再显示窗口，避免启动时先闪出错误的背景色。
            ApplyTheme(vm.State.Theme);
            var window = new MainWindow(vm); MainWindow = window;
            if (smoke is not null) window.ShowInTaskbar = false;
            window.Show();
            // 验证入口执行真实只读请求和截图；正常启动独立并行获取通知，读串维护不影响公告。
            if (smoke is not null) { await SmokeRunner.RunAsync(window, vm, smoke); Shutdown(); }
            else
            {
                using var notices = new SiteNoticeClient();
                using var presenter = new StartupNoticePresenter(window, vm, notices);
                await Task.WhenAll(vm.InitializeAsync(), presenter.RunAsync());
            }
        }
        catch (Exception ex)
        {
            if (smoke is not null) { Directory.CreateDirectory(smoke); File.WriteAllText(Path.Combine(smoke, "failure.txt"), ex.ToString()); }
            else MessageBox.Show("启动未完成：" + ex.Message, "潮汐", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (_ownsInstance) _instance?.ReleaseMutex();
        _instance?.Dispose(); base.OnExit(e);
    }

    internal static void ApplySpoilerProtection(bool enabled)
    {
        // 仅真正切换时重绘；修改字号或主题不收起已经展开的内容。
        if (Current is not null && !Equals(Current.Resources["HideSpoilers"], enabled))
            Current.Resources["HideSpoilers"] = enabled;
    }

    public static void ApplyTheme(string theme)
    {
        // 同步 WPF UI 控件主题和阅读区语义画刷，避免深色窗口中出现浅色输入框。
        var dark = theme == "dark";
        var libraryTheme = SystemParameters.HighContrast ? Wpf.Ui.Appearance.ApplicationTheme.HighContrast
            : dark ? Wpf.Ui.Appearance.ApplicationTheme.Dark : Wpf.Ui.Appearance.ApplicationTheme.Light;
        Wpf.Ui.Appearance.ApplicationThemeManager.Apply(libraryTheme, Wpf.Ui.Controls.WindowBackdropType.None, false);
        Wpf.Ui.Appearance.ApplicationAccentColorManager.Apply(
            (Color)ColorConverter.ConvertFromString(dark ? "#66C5A7" : "#13765E"), libraryTheme, false);
        var colors = dark ? new Dictionary<string, string>
        {
            ["PostGreen"]="#42D995", ["PostRed"]="#FF6B7B",
            ["CookieBackground"]="#20533F", ["CookieForeground"]="#8DE0BC", ["AdminCookieBackground"]="#612F34", ["AdminCookieForeground"]="#FFB7AD", ["SageBackground"]="#D91B37", ["OnSage"]="#FFFFFF",
            ["Canvas"]="#202124", ["Surface"]="#27292D", ["Sidebar"]="#222428", ["Text"]="#ECEEF0", ["Muted"]="#B0B6BE", ["PoBackground"]="#203E62", ["PoForeground"]="#ACD1FF",
            ["Line"]="#3D4249", ["Accent"]="#66C5A7", ["OnAccent"]="#10271E", ["AccentSoft"]="#273F34", ["Hover"]="#34373D", ["Danger"]="#F3A299", ["DangerSoft"]="#422C29", ["FavoriteStar"]="#F2C94C"
        } : new Dictionary<string, string>
        {
            ["PostGreen"]="#007548", ["PostRed"]="#C51D35",
            ["CookieBackground"]="#BDEAD8", ["CookieForeground"]="#086147", ["AdminCookieBackground"]="#FFD2CC", ["AdminCookieForeground"]="#982F2B", ["SageBackground"]="#D5102B", ["OnSage"]="#FFFFFF",
            ["Canvas"]="#F3F4F5", ["Surface"]="#FFFFFF", ["Sidebar"]="#F6F7F8", ["Text"]="#20252B", ["Muted"]="#626B74", ["PoBackground"]="#DCEBFF", ["PoForeground"]="#154E91",
            ["Line"]="#E2E5E8", ["Accent"]="#13765E", ["OnAccent"]="#FFFFFF", ["AccentSoft"]="#DCEDE5", ["Hover"]="#D8E3E0", ["Danger"]="#AE413B", ["DangerSoft"]="#FCECE8", ["FavoriteStar"]="#A87500"
        };
        foreach (var (key, color) in colors) Current.Resources[key] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
        // 主按钮成组更新颜色资源，保留库的状态模板，避免切换主题后深色字落在旧的深色底上。
        Current.Resources["AccentButtonBackground"] = Current.Resources["Accent"];
        Current.Resources["AccentButtonBackgroundPointerOver"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(dark ? "#76D2B4" : "#10664F"));
        Current.Resources["AccentButtonBackgroundPressed"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(dark ? "#5CB797" : "#0C543F"));
        foreach (var key in new[] { "AccentButtonForeground", "AccentButtonForegroundPointerOver", "AccentButtonForegroundPressed" })
            Current.Resources[key] = Current.Resources["OnAccent"];
        // 高对比度沿用系统色；饼干 ID、SAGE 文本和 PO 标签继续承担状态区分。
        if (SystemParameters.HighContrast)
        {
            foreach (var key in new[] { "Canvas", "Surface", "Sidebar", "AccentSoft", "DangerSoft", "PoBackground", "CookieBackground", "AdminCookieBackground", "SageBackground" }) Current.Resources[key] = SystemColors.WindowBrush;
            foreach (var key in new[] { "Text", "Muted", "Accent", "Danger", "Line", "FavoriteStar", "PoForeground", "CookieForeground", "AdminCookieForeground", "OnSage", "PostGreen", "PostRed" }) Current.Resources[key] = SystemColors.WindowTextBrush;
            Current.Resources["Hover"] = SystemColors.ControlBrush;
            foreach (var key in new[] { "AccentButtonBackground", "AccentButtonBackgroundPointerOver", "AccentButtonBackgroundPressed" })
                Current.Resources[key] = SystemColors.HighlightBrush;
            foreach (var key in new[] { "AccentButtonForeground", "AccentButtonForegroundPointerOver", "AccentButtonForegroundPressed" })
                Current.Resources[key] = SystemColors.HighlightTextBrush;
        }
    }
}
