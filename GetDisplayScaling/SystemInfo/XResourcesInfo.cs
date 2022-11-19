using System.Globalization;
using GetDisplayScaling.Native;

namespace GetDisplayScaling.SystemInfo;

public static class XResourcesInfo
{
    // xrdb -q | grep Xft.dpi
    public static unsafe int? GetXftDpi()
    {
        var display = LibX11.XOpenDisplay(null);
        
        var resourceStringPtr = LibX11.XResourceManagerString(display);
        
        var resourceString = resourceStringPtr == null ? string.Empty : new string(resourceStringPtr);
        
        var xftDpiValue = resourceString.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
            .Where(x => x.StartsWith("Xft.dpi"))
            .Select(x => x.Split(":", StringSplitOptions.RemoveEmptyEntries)
                .Select(y => y.Trim()).LastOrDefault())
            .FirstOrDefault();

        LibX11.XFree(resourceStringPtr);
        //LibX11.XCloseDisplay(display);
        
        return int.TryParse(xftDpiValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var xftDpi) ? xftDpi : default(int?);
    }

}