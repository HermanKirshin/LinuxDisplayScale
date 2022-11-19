namespace GetDisplayScaling.SystemInfo;

public enum WellKnownDesktopEnvironmentType
{
    Unknown,
    Gnome,
    Kde,
    Xfce,
    Lxqt,
    Mate,
    Cinnamon,
    Deepin,
}

public static class WellKnownDesktopEnvironment
{
    private static readonly Lazy<WellKnownDesktopEnvironmentType> type = new (GetEnvironmentType);

    public static WellKnownDesktopEnvironmentType Type => type.Value;
    
    private static WellKnownDesktopEnvironmentType GetEnvironmentType()
    {
        var xcd = Environment.GetEnvironmentVariable("XDG_CURRENT_DESKTOP")?.ToUpperInvariant() ?? string.Empty;

        var map = new Dictionary<WellKnownDesktopEnvironmentType, string>
        {
            { WellKnownDesktopEnvironmentType.Gnome, "GNOME" },
            { WellKnownDesktopEnvironmentType.Kde, "KDE" },
            { WellKnownDesktopEnvironmentType.Xfce, "XFCE" },
            { WellKnownDesktopEnvironmentType.Lxqt, "LXQT" },
            { WellKnownDesktopEnvironmentType.Mate, "MATE" },
            { WellKnownDesktopEnvironmentType.Cinnamon, "CINNAMON" },
            { WellKnownDesktopEnvironmentType.Deepin, "DEEPIN" }
        };

        foreach (var item in map)
        {
            if (xcd.Contains(item.Value))
                return item.Key;
        }

        return WellKnownDesktopEnvironmentType.Unknown;
    }
}