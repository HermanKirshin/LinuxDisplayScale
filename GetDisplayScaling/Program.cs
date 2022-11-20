using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using GetDisplayScaling.Native;
using GetDisplayScaling.SystemInfo;
using GSettings = GetDisplayScaling.SystemInfo.GSettings;

namespace GetDisplayScale
{
    public static unsafe class Program
    {
        public static void Main()
        {
            var scalingFactors = CalculateScalingFactors();

            var configurationString = string.Join(";",
                scalingFactors.Select(x => x.Key + "=" + x.Value.ToString("N2", CultureInfo.InvariantCulture)));
        
            Console.WriteLine(configurationString);
        }

        private static IReadOnlyDictionary<string, double> CalculateScalingFactors()
        {
            var xRandrMonitors = XRandrMonitorInfo.Enumerate();
            var gtkMonitors = GtkMonitorInfo.Enumerate();
            var waylandMonitors = WaylandMonitorInfo.Enumerate();
            var kdeWaylandMonitors = KdeWaylandMonitorInfo.Enumerate();
            var xftScale = XResourcesInfo.GetXftDpi() / 96.0;
            var variablesMap = MapEnvironmentVariables(xRandrMonitors);
                
            var waylandScaleFactors = GetWaylandScaleFactors(waylandMonitors, kdeWaylandMonitors);
            var xRandrWaylandMap = MatchWaylandMonitors(xRandrMonitors, waylandMonitors, waylandScaleFactors.Values.DefaultIfEmpty().Max());
            var gtkMap = MatchGtkMonitors(xRandrMonitors, gtkMonitors);
            
            static bool IsFractional(double value) => Math.Abs(value - Math.Truncate(value)) > 0.0001;
            static bool EqualsInteger(double value, int cmp) => !IsFractional(value) && (int)value == cmp;

            Dictionary<string, double> scalingFactors;
            
            if (waylandMonitors.Any()) // Wayland
            {
                if (GSettings.IsGnomeWayland) // Gnome
                {
                    // Gnome renders X11 in 100% and then upscales to expected size. To avoid scaling twice and getting too big UI elements set to 1.
                    scalingFactors = GSettings.HasFractionalScaling() ? 
                        xRandrMonitors.ToDictionary(x => x.Name, _ => 1.0) : 
                        xRandrMonitors.ToDictionary(x => x.Name, x => xRandrWaylandMap.TryGetValue(x, out var wayland) ? wayland.Scale : 1.0);
                }
                else if (kdeWaylandMonitors.Any()) // KDE
                {
                    if (xRandrWaylandMap.All(x => IsXrandrUpscale(x.Key, x.Value, waylandScaleFactors[x.Value])))
                    {
                        // Same as fractional scale in Gnome, app is rendered in 100% and then upscaled to expected size. In this case all xrandr area sizes are wayland ones divided by scale factor
                        scalingFactors = xRandrMonitors.ToDictionary(x => x.Name, _ => 1.0);
                    }
                    else 
                    {
                        // X11 applications are configured to scale themselves.
                        // In this case take system scaling factors - KDE provides fractional scaling via api
                        // Randr area with biggest factor is unchanged, others are divided by their scale factor and multiplied by largest scale factor, so correct scale is using largest factor everywhere
                        // I.e. for 3840x2400@1.25+1920x1080@1.75 you will get 5376x3360+1920x1080. Logical size of first display is 3072x1920, so scale factor for X11 app on first monitor should also be 1.75.

                        scalingFactors = xRandrMonitors.ToDictionary(x => x.Name, _ => waylandScaleFactors.Values.Max());
                    }
                }
                else 
                {
                    scalingFactors = xRandrMonitors.ToDictionary(x => x.Name, x => xRandrWaylandMap.TryGetValue(x, out var wayland) ? wayland.Scale : 1.0);
                }
            }
            else // X11
            {
                // Currently found two environments that may have fractional scaling using xrandr transformations - XFCE and Cinnamon
                if (xRandrMonitors.Any(x => !EqualsInteger(x.MatrixScale, 1)))
                {
                    if (WellKnownDesktopEnvironment.Type == WellKnownDesktopEnvironmentType.Xfce)
                    {
                        // XFCE fractional scaling uses same downscaling approach but it is completely broken
                        // Matrix scale transformation is set to user values from display settings as is, so R=S*M, and most applications are always rendered 100%,
                        // Gtk values are same for all monitors and are set in Appearance - window scaling, and not affected by per monitor scale in display settings. Some apps consider this value.
                        // Final scale is AppearanceWindowScale/MonitorScale, it gives 100% for 2x monitor scale and 2x display scale in apps that use gtk scalings, 
                        // 50% for 2x monitor scale in apps that do not use window scaling, 
                        // 200% for 1x monitor scale and 2x display scale in apps that use window scaling, 
                        // 100% for 1x monitor scale in apps that do not use window scaling, 
                        // 133% for 1.5x monitor scale in apps that use window scaling, 
                        // 66% for 1.5x monitor scale in apps that do not use window scaling
                        // Thus it is pointless to perform any mathematics for getting precise size and use gtk scale just to behave like other apps
                        
                        scalingFactors = xRandrMonitors.ToDictionary(x => x.Name, x => Math.Max(
                            variablesMap.TryGetValue(x, out var variableValue) ? variableValue : 1.0,
                            gtkMap.TryGetValue(x, out var gtkMonitor) ? gtkMonitor.Scale : 1.0));
                    }
                    else
                    {
                        // Unlike wayland fractional scaling Cinnamon downscales frame rendered in higher resolution, to avoid blur.
                        // For example physical UI elements size in 3840x2160 resolution with 1.25 scale is equivalent to size in 3072x1920 resolution.
                        // But rendering to 3072x1920 and then upscaling to 3840x2160 will cause weird blur. So rendered area is 
                        // increased by 2 to 6144x3840 and then downscaled to 3840x2160. 

                        // Render area size (R) is R=D*M where D - real display resolution and M - matrix scale transformation factor 
                        // Matrix scale M=2/S where S - scale factor set in system settings (1.25, 1.75 etc). 
                        // i.e. 3840x2160 * 2/1.25 = 6144x3840. 

                        // But because fractional multiplier is already applied to render area size, it should not be applied again on app-level, 
                        // app should just consider doubled dimensions of render area.

                        scalingFactors = xRandrMonitors.ToDictionary(x => x.Name, _ => 2.0);
                    }
                }
                else
                {
                    // How it can be missing? No gtk installed?
                    if (!gtkMonitors.Any())
                    {
                        if (xftScale != null)
                        {
                            scalingFactors = xRandrMonitors.ToDictionary(x => x.Name, _ => xftScale.Value);
                        }
                        else
                        {
                            // No system scaling methods used, even Xft.dpi is empty (i.e. LXQt, WSL), system settings just set env variables 
                            scalingFactors = xRandrMonitors.ToDictionary(x => x.Name, x => variablesMap.TryGetValue(x, out var variableValue) ? variableValue : 1.0);
                        }
                    }
                    else
                    {
                        if (xftScale != null)
                        {
                            // Some cases out of the box system does not support per-monitor scaling (KDE, deepin), but support fractional scaling,
                            // gtk returns rounded down values, but from Xft.dpi we can calculate correct value.
                            if (IsFractional(xftScale.Value) && !gtkMonitors.Any(x => x.Scale > xftScale))
                            {
                                scalingFactors = xRandrMonitors.ToDictionary(x => x.Name, _ => xftScale.Value);
                            }
                            else
                            {
                                // MATE, Cinnamon non-fractional setting (200%, 300%)
                                scalingFactors = xRandrMonitors.ToDictionary(x => x.Name, x => gtkMap.TryGetValue(x, out var gtkMonitor) ? gtkMonitor.Scale : xftScale.Value);
                            }
                        }
                        else
                        {
                            // No system scaling methods used, even Xft.dpi is empty (i.e. LXQt, WSL), system settings just set env variables 
                            scalingFactors = xRandrMonitors.ToDictionary(x => x.Name, x => Math.Max(
                                variablesMap.TryGetValue(x, out var variableValue) ? variableValue : 1.0,
                                gtkMap.TryGetValue(x, out var gtkMonitor) ? gtkMonitor.Scale : 1.0));
                        }
                    }
                }

                return scalingFactors;
            }

            return scalingFactors;
        }
        
        // Xwayland sets randr monitor names to XWAYLAND0, XWAYLAND1 etc. They are created in wl_output enumeration order by wl_registry_listener.
        // Not sure if it is guaranteed, so better to match by coordinates. Xwayland sets x and y for randr monitors from geometry callback from zxdg_output_v1_listener (if available) or wl_output_listener.
        // See calls to RRCrtcNotify in xwayland-output.c. Randr returns crtc coordinates in monitor geometry (see RRMonitorGetGeometry in rrmonitor.c)
        private static IReadOnlyDictionary<XRandrMonitorInfo, WaylandMonitorInfo> MatchWaylandMonitors(IReadOnlyList<XRandrMonitorInfo> xRandrMonitors, IReadOnlyList<WaylandMonitorInfo> waylandMonitors, double maxWaylandScaleFactor)
        {
            var result = new Dictionary<XRandrMonitorInfo, WaylandMonitorInfo>();

            if (xRandrMonitors.Count == 1 && waylandMonitors.Count == 1)
            {
                result.Add(xRandrMonitors.Single(), waylandMonitors.Single());
                return result;
            }

            foreach (var xRandrMonitor in xRandrMonitors)
            {
                var waylandMonitor = waylandMonitors.SingleOrDefault(x => IsEquivalentPixelPosition(xRandrMonitor, x, maxWaylandScaleFactor));
                if (waylandMonitor != null)
                    result.Add(xRandrMonitor, waylandMonitor);
            }

            // Smart matching failed, try to rely on same order
            if (xRandrMonitors.Count == waylandMonitors.Count && result.Count < xRandrMonitors.Count)
            {
                result.Clear();
                for (var i = 0; i < xRandrMonitors.Count; i++)
                    result.Add(xRandrMonitors[i], waylandMonitors[i]);
            }

            return result;
        }

        private static IReadOnlyDictionary<WaylandMonitorInfo, double> GetWaylandScaleFactors(IReadOnlyList<WaylandMonitorInfo> waylandMonitors, IReadOnlyList<KdeWaylandMonitorInfo> kdeWaylandMonitors)
        {
            var result = new Dictionary<WaylandMonitorInfo, double>();
            foreach (var waylandMonitor in waylandMonitors)
            {
                KdeWaylandMonitorInfo kdeWaylandMonitor = null;
                    
                // Names like eDP-1
                if (waylandMonitor.XdgName != null)
                    kdeWaylandMonitor = kdeWaylandMonitors.SingleOrDefault(x => x.Name == waylandMonitor.XdgName);

                // xdg_output not supported
                kdeWaylandMonitor ??= kdeWaylandMonitors.SingleOrDefault(x => x.X == waylandMonitor.X && x.Y == waylandMonitor.Y);

                result.Add(waylandMonitor, kdeWaylandMonitor?.Scale ?? waylandMonitor.Scale);
            }
            return result;
        }

        private static IReadOnlyDictionary<XRandrMonitorInfo, GtkMonitorInfo> MatchGtkMonitors(IReadOnlyList<XRandrMonitorInfo> xRandrMonitors, IReadOnlyList<GtkMonitorInfo> gtkMonitors)
        {
            var result = new Dictionary<XRandrMonitorInfo, GtkMonitorInfo>();

            if (xRandrMonitors.Count == 1 && gtkMonitors.Count == 1)
                result.Add(xRandrMonitors.Single(), gtkMonitors.Single());
            else
            {
                foreach (var xRandrMonitor in xRandrMonitors)
                {
                    var gtkMonitor = gtkMonitors.SingleOrDefault(x => x.XId == xRandrMonitor.Id);
                    
                    // just in case, likely will not be called because match by XID should work 
                    if (gtkMonitor == null)
                        gtkMonitor = gtkMonitors.FirstOrDefault(x => x.X == xRandrMonitor.X && x.Y == xRandrMonitor.Y);

                    if (gtkMonitor != null)
                        result.Add(xRandrMonitor, gtkMonitor);
                }
            }

            return result;
        }
        
        private static IReadOnlyDictionary<XRandrMonitorInfo, double> MapEnvironmentVariables(IReadOnlyList<XRandrMonitorInfo> xRandrMonitors)
        {
            var qtFactor = EnvironmentVariables.QtScaleFactor;
            var qtScreenFactors = EnvironmentVariables.QtScreenScaleFactor;
            var gdkScale = EnvironmentVariables.GtktScale;

            var result = new Dictionary<XRandrMonitorInfo, double>();
            for (var i = 0; i < xRandrMonitors.Count; i++)
            {
                var qtScreenFactor = 
                    qtScreenFactors.Where(x => x.Item1 == xRandrMonitors[i].Name).Select(x => (double?)x.Item2).FirstOrDefault() ??
                    (i < qtScreenFactors.Count && qtScreenFactors[i].Item1 == null ? qtScreenFactors[i].Item2 : default(double?));

                var value = new[] { qtFactor, qtScreenFactor, gdkScale }
                    .Where(x => x != null).OrderByDescending(x => x.Value).FirstOrDefault();

                if (value != null)
                    result.Add(xRandrMonitors[i], value.Value);
            }
        
            return result;
        }

        // Selecting min and max dimensions likely not required and dimensions can be compared directly, but just in case try to avoid potential side-effects from rotation
        private static bool IsXrandrUpscale(XRandrMonitorInfo xRandrMonitorInfo, WaylandMonitorInfo waylandMonitorInfo, double waylandScale) =>
            IsEquivalentPixelValue((int)(Math.Max(xRandrMonitorInfo.Width, xRandrMonitorInfo.Height) * waylandScale), Math.Max(waylandMonitorInfo.Width, waylandMonitorInfo.Height)) && 
            IsEquivalentPixelValue((int)(Math.Min(xRandrMonitorInfo.Width, xRandrMonitorInfo.Height) * waylandScale), Math.Min(waylandMonitorInfo.Width, waylandMonitorInfo.Height));

        // Wayland positions are always in logical coordinates. Mapped xrandr positions are wayland logical positions multiplied by biggest scale factor
        private static bool IsEquivalentPixelPosition(XRandrMonitorInfo xRandrMonitorInfo, WaylandMonitorInfo waylandMonitorInfo, double maxWaylandScaleFactor) =>
            IsEquivalentPixelValue(xRandrMonitorInfo.X, (int)(waylandMonitorInfo.X * maxWaylandScaleFactor)) && IsEquivalentPixelValue(xRandrMonitorInfo.Y, (int)(waylandMonitorInfo.Y * maxWaylandScaleFactor));

        // After wayland to randr transformations values can differ by few pixels, so instead direct comparison check if difference is lesser than 1% 
        // also if 1% is too small lets set min threshold 10px.
        private static bool IsEquivalentPixelValue(int value1, int value2) =>
            Math.Abs(value1 - value2) < Math.Max(10, Math.Min(value1, value2) / 100);
    }
}
