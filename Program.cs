using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;

namespace GetDisplayScale
{
    public static unsafe class Program
    {
        public static void Main()
        {
            var configurationString = MakeConfigurationString();

            Console.WriteLine(configurationString);

            Console.ReadKey();
        }

        public class MonitorInfo
        {
            public IntPtr Id { get; set; }
            public string Name { get; set; }
            public double MatrixScale { get; set; }
            public int GtkScale { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public int RenderWidth { get; set; }
            public int RenderHeight { get; set; }
            public bool IsPrimary { get; set; }
            public double AppScale { get; set; }
        }

        public enum WellKnownDesktopEnvironment
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

        private static string MakeConfigurationString()
        {
            var hasGnomeFractionalScale = CheckGnomeFractionalScaling();
            var display = Native.XOpenDisplay(null);
            var xftScale = GetXftScale(display);
            var monitorInfo = EnumerateRandrMonitors(display);
            FillGtkInfo(monitorInfo);

            CalculateAppScalingFactor(monitorInfo, hasGnomeFractionalScale, xftScale);

            var configurationString = string.Join(";",
                monitorInfo.Select(x => x.Name + "=" + x.AppScale.ToString("N2", CultureInfo.InvariantCulture)));
            return configurationString;
        }

        private static void CalculateAppScalingFactor(List<MonitorInfo> monitorInfo, bool hasGnomeFractionalScale, double? xftScale)
        {
            static bool IsFractional(double value) => Math.Abs(value - Math.Truncate(value)) > 0.0001;
            static bool EqualsInteger(double value, int cmp) => !IsFractional(value) && (int)value == cmp;

            monitorInfo.ForEach(x =>
            {
                
                // Default logic, for Gnome, XFCE (Window scaling 1x-2x, display scaling 1x), MATE, Cinnamon non-fractional setting (200%, 300%)
                if (EqualsInteger(x.MatrixScale, 1))
                    x.AppScale = x.GtkScale;
                else
                {
                    // Unlike gnome fractional scaling in Cinnamon and XFCE downscales frame rendered in higher resolution, to avoid blur.
                    // For example physical UI elements size in 3840x2160 resolution with 1.25 scale is equivalent to size in 3072x1920 resolution.
                    // But rendering to 3072x1920 and then upscaling to 3840x2160 will cause weird blur. So rendered area is 
                    // increased by 2 to 6144x3840 and then downscaled to 3840x2160.
                    
                    // Render area size (R) is R=D*M where D - real display resolution and M - matrix scale transformation factor 
                    // Matrix scale M=2/S where S - scale factor set in system settings (1.25, 1.75 etc). 
                    // i.e. 3840x2160 * 2/1.25 = 6144x3840. 
                    
                    // But because fractional multiplier is already applied to render area size, it should not be applied again on app-level, 
                    // app should just consider doubled dimensions of render area.

                    x.AppScale = 2;
                    
                    // XFCE fractional scaling uses same downscaling approach but it is completely broken
                    // Matrix scale transformation is set to user values from display settings as is, so R=S*M, and most applications are always rendered 100%,
                    // Gtk values are same for all monitors and are set in Appearance - window scaling, and not affected by per monitor scale in display settings. Some apps consider this value.
                    // Final scale is AppearanceWindowScale/MonitorScale, it gives 100% for 2x monitor scale and 2x display scale in apps that use gtk scalings, 
                    // 50% for 2x monitor scale in apps that do not use gtk scalings, 
                    // 200% for 1x monitor scale and 2x display scale in apps that use gtk scalings, 
                    // 100% for 1x monitor scale in apps that do not use gtk scalings, 
                    // 133% for 1.5x monitor scale in apps that use gtk scalings, 
                    // 66% for 1.5x monitor scale in apps that do not use gtk scalings
                    // Thus it is pointless to perform any mathematics for getting precise size and use gtk scale just to behave like other apps
                    if (GetWellKnownDesktopEnvironment() == WellKnownDesktopEnvironment.Xfce)
                        x.AppScale = x.GtkScale;
                }
            });

            // Gnome with wayland and fractional scale (experimental feature, configurable in ubuntu) does not use randr matrix transformation,
            // matrix scale is always 1 and system scale factor cannot be extracted from there.
            // In theory it can be calculated from render area size and real display resolution (because it is upscaling values will be lesser than 1),
            // but anyway application should not scale itself at all. It is rendered in 100% and upscaled to configured values with blur.
            // To avoid scaling twice and getting too big UI elements set to 1.s
            // By the way Gtk scale factors are rounded up display scaling values which also have no use here.
            if (hasGnomeFractionalScale)
                monitorInfo.ForEach(x => x.AppScale = 1);
            else
            {
                // Some cases out of the box system does not support per-monitor scaling (KDE, deepin), but support fractional scaling,
                // gtk returns rounded down values, but from Xft.dpi we can calculate correct value. By the way KDE and deepin also set QT_SCREEN_SCALE_FACTORS
                if (xftScale != null && IsFractional(xftScale.Value) && monitorInfo.All(x => !IsFractional(x.AppScale)))
                    monitorInfo.ForEach(x => x.AppScale = xftScale.Value);
                else
                {
                    // No system scaling methods used, even Xft.dpi is empty (i.e. LXQt, WSL), system settings just set env variables 
                    var noGtkXrandrScaleInfo = xftScale == null ||
                                               EqualsInteger(xftScale.Value, 1) &&
                                               monitorInfo.All(x => EqualsInteger(x.AppScale, 1));
                    if (noGtkXrandrScaleInfo)
                    {
                        var gdkScaleVar = Environment.GetEnvironmentVariable("GDK_SCALE");
                        var qtScaleVar = Environment.GetEnvironmentVariable("QT_SCALE_FACTOR");

                        var gdkScale = 0.0;
                        var qtScale = 0.0;
                        if (double.TryParse(gdkScaleVar, NumberStyles.Any, CultureInfo.InvariantCulture, out gdkScale) ||
                            double.TryParse(qtScaleVar, NumberStyles.Any, CultureInfo.InvariantCulture, out qtScale))
                        {
                            foreach (var info in monitorInfo)
                                info.AppScale = Math.Max(gdkScale, qtScale);
                        }
                    }
                }
            }

            monitorInfo.ForEach(x => x.AppScale = Math.Round(x.AppScale, 2));
        }

        private static void FillGtkInfo(List<MonitorInfo> monitorInfo)
        {
            Native.gtk_init_check(0, IntPtr.Zero);
            
            var isWayland = monitorInfo.Any(x => x.Name.StartsWith("XWAYLAND", StringComparison.OrdinalIgnoreCase));

            var gdkDisplay = Native.gdk_display_get_default();
            var screen = Native.gdk_display_get_default_screen(gdkDisplay);
            var monitorsCount = Native.gdk_display_get_n_monitors(gdkDisplay);
            for (var i = 0; i < monitorsCount; i++)
            {
                var monitor = Native.gdk_display_get_monitor(gdkDisplay, i);
                var scale = Native.gdk_monitor_get_scale_factor(monitor);

                MonitorInfo? info;
                if (!isWayland)
                {
                    var xid = Native.gdk_x11_screen_get_monitor_output(screen, i);
                    info = monitorInfo.SingleOrDefault(x => x.Id == xid);
                }
                else
                {
                    // On wayland gdk_x11_screen_get_monitor_output fails with segfault, so we should match monitor by some other parameters
                    // Monitor order in some cases do not match between xrandr and gtk, i.e. in VirtualBox you can get reversed order
                    // Coordinate system matches between gtk and wayland. Usable only for wayland, For X it is not so, in many cases X areas are aligned left-to-right, and randr are aligned top-to-bottom
                    // To investigate:
                    // gdk_wayland_monitor_get_wl_output returns wayland handle, but need to find a way to get monitor name from it
                    // gdk_monitor_get_connector from gtk4 returns name in format eDP-1 that does not match with XWAYLAND0 name from xrandr

                    var isPrimary = Native.gdk_monitor_is_primary(monitor);
                    Native.gdk_monitor_get_geometry(monitor, out var rectangle);
                    info = monitorInfo.FirstOrDefault(x =>
                        isPrimary && x.IsPrimary ||
                        (x.X == rectangle.x && x.Y == rectangle.y));
                }

                if (info != null)
                    info.GtkScale = scale;
            }
        }

        private static List<MonitorInfo> EnumerateRandrMonitors(Native.Display* display)
        {
            var monitorInfo = new List<MonitorInfo>();

            var rootWindow = Native.XDefaultRootWindow(display);
            var resources = Native.XRRGetScreenResources(display, rootWindow);
            var monitors = Native.XRRGetMonitors(display, rootWindow, true, out var monitorCount);
            for (var i = 0; i < monitorCount; i++)
            {
                for (var j = 0; j < monitors[i].noutput; j++)
                {
                    var outputInfo = Native.XRRGetOutputInfo(display, resources, monitors[i].outputs[j]);

                    // Cinnamon has per-monitor fractional scaling that works (unlike gnome). But I had not found api that allows retrieve these settings,
                    // they are stored in ~/.config/cinnamon-monitors.xml (same format as ubuntu ~/.config/monitors.xml).
                    // Configuration tool calls xrandr to reconfigure, so we can extract these settings from transformation matrix.
                    // gtk scale factor should be divided by this value to get correct fractional scale.
                    Native.XRRGetCrtcTransform(display, outputInfo->crtc, out var transformAttributes);

                    var matrix = new Matrix4x4(
                        transformAttributes->currentTransform.matrix00, transformAttributes->currentTransform.matrix01,
                        transformAttributes->currentTransform.matrix02, 0.0f,
                        transformAttributes->currentTransform.matrix10, transformAttributes->currentTransform.matrix11,
                        transformAttributes->currentTransform.matrix12, 0.0f,
                        transformAttributes->currentTransform.matrix20, transformAttributes->currentTransform.matrix21,
                        transformAttributes->currentTransform.matrix22, 0.0f,
                        0.0f, 0.0f, 0.0f, 1.0f);

                    Matrix4x4.Decompose(matrix, out var scale, out var _, out var _);

                    monitorInfo.Add(new MonitorInfo
                    {
                        Id = monitors[i].outputs[j].xid,
                        Name = new string((sbyte*)outputInfo->name, 0, outputInfo->nameLen),
                        MatrixScale = Math.Max(scale.X, scale.Y),
                        X = monitors[i].x,
                        Y = monitors[i].y,
                        RenderWidth = monitors[i].width,
                        RenderHeight = monitors[i].height,
                        IsPrimary = monitors[i].primary != 0
                    });

                    Native.XFree(transformAttributes);
                    Native.XRRFreeOutputInfo(outputInfo);
                }
            }

            Native.XRRFreeScreenResources(resources);

            return monitorInfo;
        }

        // xrdb -q | grep Xft.dpi
        private static double? GetXftScale(Native.Display* display)
        {
            var resourceString = Native.XResourceManagerString(display);
            var xftValue = resourceString?.Split(Environment.NewLine.ToCharArray())
                ?.FirstOrDefault(x => x.StartsWith("Xft.dpi"))?.Split(":")?.Select(x => x.Trim())?.LastOrDefault();
            var xftScale = xftValue == null
                ? default(double?)
                : int.Parse(xftValue, NumberStyles.Any, CultureInfo.InvariantCulture) / 96.0;
            return xftScale;
        }

        // terminal command - check if "gsettings get org.gnome.mutter experimental-features" contains "['scale-monitor-framebuffer']"
        private static bool CheckGnomeFractionalScaling()
        {
            var schemaSource = Native.g_settings_schema_source_get_default();
            if (schemaSource == null)
                return false;

            var schema = Native.g_settings_schema_source_lookup(schemaSource, "org.gnome.mutter", false);
            if (schema == null)
                return false;

            var settings = Native.g_settings_new("org.gnome.mutter");
            if (settings == null)
                return false;
            
            var stringArrayPtr = Native.g_settings_get_strv(settings, "experimental-features");
            while (true)
            {
                var stringPtr = Marshal.ReadIntPtr(stringArrayPtr);
                if (stringPtr == IntPtr.Zero)
                    break;
                stringArrayPtr += IntPtr.Size;

                var value = Marshal.PtrToStringAnsi(stringPtr);
                if (value == "scale-monitor-framebuffer")
                    return true;
            }

            return false;
        }

        private static WellKnownDesktopEnvironment GetWellKnownDesktopEnvironment()
        {
            var xcd = Environment.GetEnvironmentVariable("XDG_CURRENT_DESKTOP")?.ToUpperInvariant() ?? string.Empty;

            var map = new Dictionary<WellKnownDesktopEnvironment, string>
            {
                { WellKnownDesktopEnvironment.Gnome, "GNOME" },
                { WellKnownDesktopEnvironment.Kde, "KDE" },
                { WellKnownDesktopEnvironment.Xfce, "XFCE" },
                { WellKnownDesktopEnvironment.Lxqt, "LXQT" },
                { WellKnownDesktopEnvironment.Mate, "MATE" },
                { WellKnownDesktopEnvironment.Cinnamon, "CINNAMON" },
                { WellKnownDesktopEnvironment.Deepin, "DEEPIN" }
            };

            foreach (var item in map)
            {
                if (xcd.Contains(item.Value))
                    return item.Key;
            }

            return WellKnownDesktopEnvironment.Unknown;
        }
    }
}
