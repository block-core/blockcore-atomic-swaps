namespace Blockcore.AtomicSwaps.Website.ThemeBase.libs;

// Base type class for theme settings
class ThemeBase
{
    public string ThemeDir { get; set; }

    public string Direction { get; set; }

    public bool ModeSwitchEnabled { get; set; }

    public string ModeDefault { get; set; }

    public string AssetsDir { get; set; }

    public ThemeAssets Assets { get; set; }

    public SortedDictionary<string, SortedDictionary<string, string[]>> Vendors { get; set; }
}
