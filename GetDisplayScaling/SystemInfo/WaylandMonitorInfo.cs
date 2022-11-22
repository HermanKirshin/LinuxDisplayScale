using System.Runtime.InteropServices;
using GetDisplayScaling.Native;

namespace GetDisplayScaling.SystemInfo;

public sealed class WaylandMonitorInfo
{
    public int X { get; private set; }
    public int Y { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
            
    public int? XdgLogicalX { get; private set; }
    public int? XdgLogicalY { get; private set; }
    public int? XdgLogicalWidth { get; private set; }
    public int? XdgLogicalHeight { get; private set; }
    public string XdgName { get; private set; }
    public string XdgDescription { get; private set; }

    public int Scale { get; private set; }
            
    public string Manufacturer { get; private set; }
    public string Model { get; private set; }
            
    public int WidthMm { get; private set; }
    public int HeightMm { get; private set; }
            
    private WaylandMonitorInfo()
    {
    }

    public static IReadOnlyList<WaylandMonitorInfo> Enumerate() => new MonitorList().List;
    
    private class MonitorList
    {
        public IReadOnlyList<WaylandMonitorInfo> List => list;

        private readonly List<WaylandMonitorInfo> list = new();
        private Dictionary<IntPtr, int> outputMap = new();
        private List<IntPtr> xdgOutputs = new();
        private unsafe zxdg_output_manager_v1* xdgOutputManager;
        
        public unsafe MonitorList()
        {
            if (!LibWayland.Exists)
                return;
            
            var display = LibWayland.wl_display_connect(null);
            if (display == null)
                return;
            
            var registry = LibWayland.wl_display_get_registry(display);

            var registryListener = new wl_registry_listener(OnRegistryGlobal, OnRegistryGlobalRemove, out var registryListenerKeepAlive);
        
            var code = LibWayland.wl_registry_add_listener(registry, &registryListener, null);
            //if (code == 0)
            //    return result;
        
            LibWayland.wl_display_roundtrip(display); // list outputs and get xdgmanager

            var outputListener = new wl_output_listener(OnOutputGeometry, OnOutputMode, OnOutputDone, OnOutputScale, out var outputKeepAlive);
            var xdgListener = new zxdg_output_v1_listener(OnXdgOutputLogicalPosition, OnXdgOutputLogicalSize, OnXdgOutputDone, OnXdgOutputName, OnXdgOutputDescription, out var xdgOutputKeepAlive);
            foreach (var pair in outputMap)
            {
                code = LibWayland.wl_output_add_listener((wl_output*)pair.Key, &outputListener, (void*)pair.Value);
                //if (code == 0)
                //    return result;

                if (xdgOutputManager != null)
                {
                    var xdgOutput = LibWayland.zxdg_output_manager_v1_get_xdg_output(xdgOutputManager, (wl_output*)pair.Key);
                    if (xdgOutput != null)
                    {
                        xdgOutputs.Add((IntPtr)xdgOutput);
                        code = LibWayland.zxdg_output_v1_add_listener(xdgOutput, &xdgListener, (void*)pair.Value);
                        //if (code == 0)
                        //    return result;
                    }
                }
            }

            LibWayland.wl_display_roundtrip(display); // run output listeners
            
            xdgOutputs.ForEach(x => LibWayland.zxdg_output_v1_destroy((zxdg_output_v1*)x));
            if (xdgOutputManager != null)
                LibWayland.zxdg_output_manager_v1_destroy(xdgOutputManager);

            foreach (var output in outputMap.Keys)
                LibWayland.wl_output_destroy((wl_output*)output);
            if (registry != null)
                LibWayland.wl_registry_destroy(registry);

            if (display != null)
                LibWayland.wl_display_disconnect(display);

            xdgOutputKeepAlive();
            outputKeepAlive();
            registryListenerKeepAlive();
        }


        private unsafe void OnXdgOutputLogicalPosition(void* data, zxdg_output_v1* zxdg_output_v1, int x, int y)
        {
            var index = (int)data;
            var info = list[index];

            info.XdgLogicalX = x;
            info.XdgLogicalY = y;
        }
        
        private unsafe void OnXdgOutputLogicalSize(void *data, zxdg_output_v1 *zxdg_output_v1, int width, int height)
        {
            var index = (int)data;
            var info = list[index];

            info.XdgLogicalWidth = width;
            info.XdgLogicalHeight = height;
        }

        private unsafe void OnXdgOutputDone(void *data, zxdg_output_v1 *zxdg_output_v1)
        {
        }

        private unsafe void OnXdgOutputName(void *data,zxdg_output_v1 *zxdg_output_v1, [MarshalAs(UnmanagedType.LPStr)] string  name)
        {
            var index = (int)data;
            var info = list[index];

            info.XdgName = name;
        }

        private unsafe void OnXdgOutputDescription(void *data, zxdg_output_v1 *zxdg_output_v1, [MarshalAs(UnmanagedType.LPStr)] string description)
        {
            var index = (int)data;
            var info = list[index];

            info.XdgDescription = description;
        }

        private unsafe void OnOutputGeometry(void* data, wl_output* wl_output, int x, int y, int physical_width,
            int physical_height, int subpixel, [MarshalAs(UnmanagedType.LPStr)] string make,
            [MarshalAs(UnmanagedType.LPStr)] string model, int transform)
        {
            var index = (int)data;
            var info = list[index];
                
            info.X = x;
            info.Y = y;
            info.WidthMm = physical_width;
            info.HeightMm = physical_height;
            info.Manufacturer = make;
            info.Model = model;
        }

        private unsafe void OnOutputMode(void* data, wl_output* wl_output, uint flags, int width, int height,
            int refresh)
        {
            var index = (int)data;
            var info = list[index];

            if ((flags & LibWayland.WL_OUTPUT_MODE_CURRENT) != 0)
            {
                info.Width = width;
                info.Height = height;
            }
        }

        private unsafe void OnOutputDone(void* data, wl_output* wl_output)
        {
        }

        private unsafe void OnOutputScale(void* data, wl_output* wl_output, int factor)
        {
            var index = (int)data;
            var info = list[index];
                
            info.Scale = factor;
        }
        
        private unsafe void OnRegistryGlobal(void* data, wl_registry* wlRegistry, uint name, [MarshalAs(UnmanagedType.LPStr)] string @interface, uint version)
        {
            if (@interface == new string(LibWayland.wl_output_interface->name))
            {
                var output = (wl_output*)LibWayland.wl_registry_bind(wlRegistry, name, LibWayland.wl_output_interface, Math.Min(version, 2));
                outputMap.Add((IntPtr)output, list.Count);
                list.Add(new WaylandMonitorInfo());
            }

            if (@interface == new string(LibWayland.zxdg_output_manager_v1_interface->name))
            {
                if (xdgOutputManager == null)
                    xdgOutputManager = (zxdg_output_manager_v1*)LibWayland.wl_registry_bind(wlRegistry, name, LibWayland.zxdg_output_manager_v1_interface, Math.Min(version, 2));
                //else
                //{ }
            }
        }

        private unsafe void OnRegistryGlobalRemove(void* data, wl_registry* wlRegistry, uint name)
        {
        }
    }
}