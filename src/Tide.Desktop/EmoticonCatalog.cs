using System.IO;
using System.Text.Json;

namespace Tide.Desktop;

/// <summary>随 EXE 内置的静态资料；原始 Unicode 和空白供菜单插入及最近使用记录共用。</summary>
internal sealed record EmoticonEntry(string Text, string Name, string Source)
{
    public string Display => Name.Length > 0 ? Name : Text;
}

internal static class EmoticonCatalog
{
    public static IReadOnlyList<EmoticonEntry> Entries { get; } = Load();
    public static List<EmoticonEntry> Recent(IEnumerable<string> recent) => recent.Distinct()
        .Select(text => Entries.FirstOrDefault(e => e.Text == text)).OfType<EmoticonEntry>().Take(20).ToList();

    public static List<string> Remember(IEnumerable<string> recent, string text) =>
        Recent(new[] { text }.Concat(recent)).Select(e => e.Text).ToList();

    private static IReadOnlyList<EmoticonEntry> Load()
    {
        using var stream = typeof(EmoticonCatalog).Assembly.GetManifestResourceStream("Tide.Emoticons.json")
            ?? throw new InvalidDataException("缺少内置颜文字库。");
        var entries = JsonSerializer.Deserialize<List<EmoticonEntry>>(stream) ?? throw new InvalidDataException("颜文字库为空。");
        return entries.AsReadOnly();
    }
}
