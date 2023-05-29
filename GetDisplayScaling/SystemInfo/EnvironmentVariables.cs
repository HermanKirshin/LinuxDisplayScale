using System.Globalization;

namespace GetDisplayScaling.SystemInfo;

public static class EnvironmentVariables
{
    public static double? QtScaleFactor => double.TryParse(Environment.GetEnvironmentVariable("QT_SCALE_FACTOR"), NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ? result : null;

    // in theory it should be integer, but some environments line LXQt can set fractional values
    public static double? GtktScale => double.TryParse(Environment.GetEnvironmentVariable("GDK_SCALE"), NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ? result : null;

    public static IReadOnlyList<(string, double)> QtScreenScaleFactor
    {
        get
        {
            var qtScreenFactorsVar = Environment.GetEnvironmentVariable("QT_SCREEN_SCALE_FACTORS") ?? string.Empty;
            var parts = qtScreenFactorsVar.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            return parts
                .Select(part => part.Split("=".ToCharArray()))
                .Select(itemParts => (
                    itemParts.Length > 1 ? itemParts.First() : null,
                    double.TryParse(itemParts.Last(), NumberStyles.Any, CultureInfo.InvariantCulture, out var value) ? value : 1))
                .ToList();
        }
    }
}