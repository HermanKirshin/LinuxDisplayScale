using System.Runtime.InteropServices;
using System.Text;
using GetDisplayScaling.Native;

namespace GetDisplayScaling.SystemInfo;

public sealed class KdeWaylandMonitorInfo
{
    public int X { get; private set; }
    public int Y { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }

    public double Scale { get; private set; } = 1;
            
    public string Manufacturer { get; private set; }
    public string Model { get; private set; }
            
    public int WidthMm { get; private set; }
    public int HeightMm { get; private set; }
    public string Name { get; private set; }
    
    private KdeWaylandMonitorInfo()
    {
    }

    public static IReadOnlyList<KdeWaylandMonitorInfo> Enumerate() => new MonitorList().List;
    
    private class MonitorList
    {
        public IReadOnlyList<KdeWaylandMonitorInfo> List => list;

        private readonly List<KdeWaylandMonitorInfo> list = new();
        private readonly List<IntPtr> devices = new();
        private readonly List<IntPtr> modes = new();
        private readonly List<IntPtr> allModes = new();
        private readonly Dictionary<IntPtr, (int, int)> sizeMap = new();
        private readonly unsafe kde_output_device_mode_v2_listener* modeListenerPointer;
        private readonly List<KdeWaylandMonitorInfo> disabled = new();
        
        public unsafe MonitorList()
        {
            if (!LibWayland.Exists || !EnvironmentVariables.IsWayland)
                return;
            
            var display = LibWayland.wl_display_connect(null);

            var registry = LibWayland.wl_display_get_registry(display);

            var registryListener = new wl_registry_listener(OnRegistryGlobal, OnRegistryGlobalRemove, out var registryListenerKeepAlive);
        
            var code = LibWayland.wl_registry_add_listener(registry, &registryListener, null);
            //if (code == 0)
            //    return result;
        
            LibWayland.wl_display_roundtrip(display); // get devices

            var deviceListener = new kde_output_device_v2_listener(OnDeviceGeometry, OnDeviceCurrentMode, OnDeviceMode, OnDeviceDone, OnDeviceScale, OnDeviceEdid, OnDeviceEnabled, OnDeviceUuid, OnDeviceSerialNumber, OnDeviceEisaId, OnDeviceCapabilities, OnDeviceOverscan, OnDeviceVrrPolicy, OnDeviceRgbRange, OnDeviceName, out var deviceKeepAlive);

            var modeListener = new kde_output_device_mode_v2_listener(OnModeSize, OnModeRefresh, OnModePreferred, OnModeRemoved, out var modeKeepAlive);
            modeListenerPointer = &modeListener;
            
            for (var i = 0; i < devices.Count; i++)
            {
                code = LibWayland.kde_output_device_v2_add_listener((kde_output_device_v2*)devices[i], &deviceListener, (void*)i);
                //if (code == 0)
                //    return result;
            }

            LibWayland.wl_display_roundtrip(display); // run listeners

            for (var i = 0; i < modes.Count; i++)
            {
                if (sizeMap.TryGetValue(modes[i], out var size))
                {
                    list[i].Width = size.Item1;
                    list[i].Height = size.Item2;
                }
            }
            
            devices.ForEach(x => LibWayland.kde_output_device_v2_destroy((kde_output_device_v2*)x));
            allModes.ForEach(x => LibWayland.kde_output_device_mode_v2_destroy((kde_output_device_mode_v2*)x));
            
            if (registry != null)
                LibWayland.wl_registry_destroy(registry);

            if (display != null)
                LibWayland.wl_display_disconnect(display);

            modeKeepAlive();
            deviceKeepAlive();
            registryListenerKeepAlive();
            
            disabled.ForEach(x => list.Remove(x));
        }

        private unsafe void OnModeSize(void* data, kde_output_device_mode_v2* kde_output_device_mode_v2, int width, int height)
        {
            var mode = (IntPtr)data;

            if (!sizeMap.ContainsKey(mode))
                sizeMap.Add(mode, (width, height));
            //else 
            //{}
        }

        private unsafe void OnModeRefresh(void *data, kde_output_device_mode_v2 *kde_output_device_mode_v2, int refresh)
        {
            
        }

        private unsafe void OnModePreferred(void *data, kde_output_device_mode_v2 *kde_output_device_mode_v2)
        {
            
        }

        private unsafe void OnModeRemoved(void *data, kde_output_device_mode_v2 *kde_output_device_mode_v2)
        {
            
        }

        private unsafe void OnDeviceGeometry(void* data, kde_output_device_v2* kde_output_device_v2, int x, int y, int physical_width, int physical_height, int subpixel, [MarshalAs(UnmanagedType.LPStr)] string make, [MarshalAs(UnmanagedType.LPStr)] string model, int transform)
        {
            var info = list[(int)data];

            info.X = x;
            info.Y = y;
            info.WidthMm = physical_width;
            info.HeightMm = physical_height;
            info.Manufacturer = make;
            info.Model = model;
        }
        
        private unsafe void OnDeviceCurrentMode(void* data, kde_output_device_v2* kde_output_device_v2, kde_output_device_mode_v2* mode)
        {
            //if (modes[(int)data] != IntPtr.Zero)
            //{}

            modes[(int)data] = (IntPtr)mode;
        }

        private unsafe void OnDeviceMode(void* data, kde_output_device_v2* kde_output_device_v2, kde_output_device_mode_v2* mode)
        {
            allModes.Add((IntPtr)mode);
            var code = LibWayland.kde_output_device_mode_v2_add_listener(mode, modeListenerPointer, mode);
            //if(code != 0)
            //{}
        }

        private unsafe void OnDeviceDone(void* data, kde_output_device_v2* kde_output_device_v2)
        {
            
        }

        private unsafe void OnDeviceScale(void* data, kde_output_device_v2* kde_output_device_v2, wl_fixed_t factor)
        {
            var info = list[(int)data];
            
            info.Scale = factor;
        }

        private unsafe void OnDeviceEdid(void* data, kde_output_device_v2* kde_output_device_v2, [MarshalAs(UnmanagedType.LPStr)] string raw)
        {
        }

        private unsafe void OnDeviceEnabled(void* data, kde_output_device_v2* kde_output_device_v2, int enabled)
        {
            var info = list[(int)data];
            
            if (enabled == 0)
                disabled.Add(info);
        }

        private unsafe void OnDeviceUuid(void* data, kde_output_device_v2* kde_output_device_v2, [MarshalAs(UnmanagedType.LPStr)] string uuid)
        {
        }

        private unsafe void OnDeviceSerialNumber(void* data, kde_output_device_v2* kde_output_device_v2, [MarshalAs(UnmanagedType.LPStr)] string serialNumber)
        {
        }

        private unsafe void OnDeviceEisaId(void* data, kde_output_device_v2* kde_output_device_v2, [MarshalAs(UnmanagedType.LPStr)] string eisaId)
        {
        }

        private unsafe void OnDeviceCapabilities(void* data, kde_output_device_v2* kde_output_device_v2, uint flags)
        {
            
        }

        private unsafe void OnDeviceOverscan(void* data, kde_output_device_v2* kde_output_device_v2, uint overscan)
        {
            
        }

        private unsafe void OnDeviceVrrPolicy(void* data, kde_output_device_v2* kde_output_device_v2, uint vrr_policy)
        {
            
        }

        private unsafe void OnDeviceRgbRange(void* data, kde_output_device_v2* kde_output_device_v2, uint rgb_range)
        {
            
        }

        private unsafe void OnDeviceName(void* data, kde_output_device_v2* kde_output_device_v2, [MarshalAs(UnmanagedType.LPStr)] string name)
        {
            var info = list[(int)data];

            info.Name = name;
        }
        
        private unsafe void OnRegistryGlobal(void* data, wl_registry* wlRegistry, uint name, [MarshalAs(UnmanagedType.LPStr)] string @interface, uint version)
        {
            if (@interface == new string(LibWayland.kde_output_device_v2_interface->name))
            {
                var device = (kde_output_device_v2*)LibWayland.wl_registry_bind(wlRegistry, name, LibWayland.kde_output_device_v2_interface, Math.Min(version, 2));
                devices.Add((IntPtr)device);
                modes.Add(IntPtr.Zero);
                list.Add(new KdeWaylandMonitorInfo());
            }
        }

        private unsafe void OnRegistryGlobalRemove(void* data, wl_registry* wlRegistry, uint name)
        {
        }
    }
}