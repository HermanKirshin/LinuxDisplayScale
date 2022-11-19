using System.Numerics;
using GetDisplayScaling.Native;

namespace GetDisplayScaling.SystemInfo;

public sealed class XRandrMonitorInfo
{
    public IntPtr Id { get; }
    public string Name { get; }
    public double MatrixScale { get; }
            
    public int X { get; }
    public int Y { get; }
    public int Width { get; }
    public int Height { get; }
    public int WidthMm { get; }
    public int HeightMm { get; }
            
    public bool IsPrimary { get; }

    private unsafe XRandrMonitorInfo(Display* display, RROutput output, XRRMonitorInfo* monitor, XRRScreenResources* resources)
    {
        var outputInfo = LibXRandr.XRRGetOutputInfo(display, resources, output);

        // Cinnamon has per-monitor fractional scaling. But I had not found api that allows retrieve these settings,
        // they are stored in ~/.config/cinnamon-monitors.xml (same format as ubuntu ~/.config/monitors.xml).
        // Configuration tool calls xrandr to reconfigure, so we can extract these settings from transformation matrix.
        // gtk scale factor should be divided by this value to get correct fractional scale.
        LibXRandr.XRRGetCrtcTransform(display, outputInfo->crtc, out var transformAttributes);

        var matrix = new Matrix4x4(
            transformAttributes->currentTransform.M00, transformAttributes->currentTransform.M01,
            transformAttributes->currentTransform.M02, 0.0f,
            transformAttributes->currentTransform.M10, transformAttributes->currentTransform.M11,
            transformAttributes->currentTransform.M12, 0.0f,
            transformAttributes->currentTransform.M20, transformAttributes->currentTransform.M21,
            transformAttributes->currentTransform.M22, 0.0f,
            0.0f, 0.0f, 0.0f, 1.0f);

        Matrix4x4.Decompose(matrix, out var scale, out _, out _);

        var properties = LibXRandr.XRRListOutputProperties(display, output, out var propertyCount);
        if (properties != null)
        {
            for (var i = 0; i < propertyCount; i++)
            {
                var propertyNamePtr = LibX11.XGetAtomName(display, properties[i]);
                var propertyName = new string(propertyNamePtr);


                LibX11.XFree(propertyNamePtr);
            }
        }
        LibX11.XFree(properties);

        Id = output.xid;
        Name = outputInfo->name == null ? null : new string((sbyte*)outputInfo->name, 0, outputInfo->nameLen);
        MatrixScale = Math.Max(scale.X, scale.Y);
        X = monitor->x;
        Y = monitor->y;
        Width = monitor->width;
        Height = monitor->height;
        WidthMm = (int)outputInfo->mm_width;
        HeightMm = (int)outputInfo->mm_height;
        IsPrimary = monitor->primary != 0;

        LibX11.XFree(transformAttributes);
        LibXRandr.XRRFreeOutputInfo(outputInfo);
    }

    public static unsafe IReadOnlyList<XRandrMonitorInfo> Enumerate()
    {
        var result = new List<XRandrMonitorInfo>();

        if (!LibX11.Exists || !LibXRandr.Exists)
            return result;

        var rootWindow = LibX11.XDefaultRootWindow(LibX11.Display);
        var resources = LibXRandr.XRRGetScreenResources(LibX11.Display, rootWindow);
        var monitors = LibXRandr.XRRGetMonitors(LibX11.Display, rootWindow, true, out var monitorCount);
        for (var i = 0; i < monitorCount; i++)
        {
            for (var j = 0; j < monitors[i].noutput; j++)
                result.Add(new XRandrMonitorInfo(LibX11.Display, monitors[i].outputs[j], &monitors[i], resources));
        }

        LibXRandr.XRRFreeScreenResources(resources);

        return result.AsReadOnly();
    }
}