using GetDisplayScaling.Native;

namespace GetDisplayScaling.SystemInfo;

public sealed class GtkMonitorInfo
{
    public int X { get; }
    public int Y { get; }
    public int Width { get; }
    public int Height { get; }
            
    public bool IsPrimary { get; }
            
    public int Scale { get; }
            
    public IntPtr? XId { get; }
            
    public string Manufacturer { get; }
    public string Model { get; }
            
    public int WidthMm { get; }
    public int HeightMm { get; }

    private unsafe GtkMonitorInfo(GdkMonitor* monitor, IntPtr? xid)
    {
        LibGdk.gdk_monitor_get_geometry(monitor, out var geometry);

        Scale = LibGdk.gdk_monitor_get_scale_factor(monitor);
        XId = xid;
        IsPrimary = LibGdk.gdk_monitor_is_primary(monitor);
        X = geometry.x;
        Y = geometry.y;
        Width = geometry.width;
        Height = geometry.height;
        Manufacturer = LibGdk.gdk_monitor_get_manufacturer(monitor);
        Model = LibGdk.gdk_monitor_get_model(monitor);
        WidthMm = LibGdk.gdk_monitor_get_width_mm(monitor);
        HeightMm = LibGdk.gdk_monitor_get_height_mm(monitor);
    }

    public static unsafe IReadOnlyList<GtkMonitorInfo> Enumerate(bool isWayland)
    {
        var result = new List<GtkMonitorInfo>();

        if (!LibGtk.Exists || !LibGdk.Exists)
            return result;
        
        LibGtk.gtk_init_check(0, IntPtr.Zero);
            
        var display = LibGdk.gdk_display_get_default();
        if (display == null)
            return result;
        
        var screen = LibGdk.gdk_display_get_default_screen(display);
        if (screen == null)
            return result;
        var monitorsCount = LibGdk.gdk_display_get_n_monitors(display);
        
        for (var i = 0; i < monitorsCount; i++)
        {
            var monitor = LibGdk.gdk_display_get_monitor(display, i);
            if (monitor == null)
                continue;
            
            // On wayland gdk_x11_screen_get_monitor_output fails with segfault, so we cannot get XID to match gtk monitor with xrandr one - get scale factors via wayland api instead
            var xid = isWayland ? default(IntPtr?) : LibGdk.gdk_x11_screen_get_monitor_output(screen, i);
            result.Add(new GtkMonitorInfo(monitor, xid));
        }

        return result.AsReadOnly();
    }
}