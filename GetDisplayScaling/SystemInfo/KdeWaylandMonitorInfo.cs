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
        public IReadOnlyList<KdeWaylandMonitorInfo> List => myList;

        private readonly List<KdeWaylandMonitorInfo> myList = new();
        private readonly List<IntPtr> myDevices = new();
        private readonly List<IntPtr> myModes = new();
        private readonly List<IntPtr> myAllModes = new();
        private readonly Dictionary<IntPtr, (int, int)> mySizeMap = new();
        private readonly unsafe kde_output_device_mode_v2_listener* myModeListenerPointer;
        private readonly List<KdeWaylandMonitorInfo> myDisabled = new();

        public unsafe MonitorList()
        {
            if (!LibWayland.Exists)
                return;

            var display = LibWayland.wl_display_connect(null);
            if (display == null) // X11
                return;

            var registry = LibWayland.wl_display_get_registry(display);
            if (registry == null)
            {
                Console.WriteLine($"{nameof(LibWayland.wl_display_get_registry)} failed");
                LibWayland.wl_display_disconnect(display);
                return;
            }

            var registryListener = new wl_registry_listener(OnRegistryGlobal, OnRegistryGlobalRemove, out var registryListenerKeepAlive);

            LibWayland.wl_registry_add_listener(registry, &registryListener, null);

            LibWayland.wl_display_roundtrip(display); // get devices

            var deviceListener = new kde_output_device_v2_listener(OnDeviceGeometry, OnDeviceCurrentMode, OnDeviceMode, OnDeviceDone, OnDeviceScale, OnDeviceEdid, OnDeviceEnabled, OnDeviceUuid, OnDeviceSerialNumber, OnDeviceEisaId, OnDeviceCapabilities, OnDeviceOverscan, OnDeviceVrrPolicy, OnDeviceRgbRange, OnDeviceName, out var deviceKeepAlive);

            var modeListener = new kde_output_device_mode_v2_listener(OnModeSize, OnModeRefresh, OnModePreferred, OnModeRemoved, out var modeKeepAlive);
            myModeListenerPointer = &modeListener;

            for (var i = 0; i < myDevices.Count; i++)
            {
                LibWayland.kde_output_device_v2_add_listener((kde_output_device_v2*)myDevices[i], &deviceListener, (void*)i);
            }

            LibWayland.wl_display_roundtrip(display); // run listeners

            for (var i = 0; i < myModes.Count; i++)
            {
                if (mySizeMap.TryGetValue(myModes[i], out var size))
                {
                    myList[i].Width = size.Item1;
                    myList[i].Height = size.Item2;
                }
            }

            modeKeepAlive();
            deviceKeepAlive();
            registryListenerKeepAlive();

            myDisabled.ForEach(x => myList.Remove(x));

            myDevices.ForEach(x => LibWayland.kde_output_device_v2_destroy((kde_output_device_v2*)x));
            myAllModes.ForEach(x => LibWayland.kde_output_device_mode_v2_destroy((kde_output_device_mode_v2*)x));
            LibWayland.wl_registry_destroy(registry);
            LibWayland.wl_display_disconnect(display);
        }

        private unsafe void OnModeSize(void* data, kde_output_device_mode_v2* kdeOutputDeviceModeV2, int width, int height)
        {
            var mode = (IntPtr)data;

            if (!mySizeMap.ContainsKey(mode))
                mySizeMap.Add(mode, (width, height));
            //else
            //{}
        }

        private unsafe void OnModeRefresh(void* data, kde_output_device_mode_v2* kdeOutputDeviceModeV2, int refresh)
        {
        }

        private unsafe void OnModePreferred(void* data, kde_output_device_mode_v2* kdeOutputDeviceModeV2)
        {
        }

        private unsafe void OnModeRemoved(void* data, kde_output_device_mode_v2* kdeOutputDeviceModeV2)
        {
        }

        private unsafe void OnDeviceGeometry(void* data, kde_output_device_v2* kdeOutputDeviceV2, int x, int y, int physicalWidth, int physicalHeight, int subpixel, [MarshalAs(UnmanagedType.LPStr)] string make, [MarshalAs(UnmanagedType.LPStr)] string model, int transform)
        {
            var info = myList[(int)data];

            info.X = x;
            info.Y = y;
            info.WidthMm = physicalWidth;
            info.HeightMm = physicalHeight;
            info.Manufacturer = make;
            info.Model = model;
        }

        private unsafe void OnDeviceCurrentMode(void* data, kde_output_device_v2* kdeOutputDeviceV2, kde_output_device_mode_v2* mode)
        {
            myModes[(int)data] = (IntPtr)mode;
        }

        private unsafe void OnDeviceMode(void* data, kde_output_device_v2* kdeOutputDeviceV2, kde_output_device_mode_v2* mode)
        {
            myAllModes.Add((IntPtr)mode);
            var code = LibWayland.kde_output_device_mode_v2_add_listener(mode, myModeListenerPointer, mode);
            if (code != 0)
                Console.WriteLine($"{nameof(LibWayland.kde_output_device_mode_v2_add_listener)} failed: {code}");
        }

        private unsafe void OnDeviceDone(void* data, kde_output_device_v2* kdeOutputDeviceV2)
        {
        }

        private unsafe void OnDeviceScale(void* data, kde_output_device_v2* kdeOutputDeviceV2, wl_fixed_t factor)
        {
            var info = myList[(int)data];

            info.Scale = factor;
        }

        private unsafe void OnDeviceEdid(void* data, kde_output_device_v2* kdeOutputDeviceV2, [MarshalAs(UnmanagedType.LPStr)] string raw)
        {
        }

        private unsafe void OnDeviceEnabled(void* data, kde_output_device_v2* kdeOutputDeviceV2, int enabled)
        {
            var info = myList[(int)data];

            if (enabled == 0)
                myDisabled.Add(info);
        }

        private unsafe void OnDeviceUuid(void* data, kde_output_device_v2* kdeOutputDeviceV2, [MarshalAs(UnmanagedType.LPStr)] string uuid)
        {
        }

        private unsafe void OnDeviceSerialNumber(void* data, kde_output_device_v2* kdeOutputDeviceV2, [MarshalAs(UnmanagedType.LPStr)] string serialNumber)
        {
        }

        private unsafe void OnDeviceEisaId(void* data, kde_output_device_v2* kdeOutputDeviceV2, [MarshalAs(UnmanagedType.LPStr)] string eisaId)
        {
        }

        private unsafe void OnDeviceCapabilities(void* data, kde_output_device_v2* kdeOutputDeviceV2, uint flags)
        {

        }

        private unsafe void OnDeviceOverscan(void* data, kde_output_device_v2* kdeOutputDeviceV2, uint overscan)
        {

        }

        private unsafe void OnDeviceVrrPolicy(void* data, kde_output_device_v2* kdeOutputDeviceV2, uint vrrPolicy)
        {

        }

        private unsafe void OnDeviceRgbRange(void* data, kde_output_device_v2* kdeOutputDeviceV2, uint rgbRange)
        {

        }

        private unsafe void OnDeviceName(void* data, kde_output_device_v2* kdeOutputDeviceV2, [MarshalAs(UnmanagedType.LPStr)] string name)
        {
            var info = myList[(int)data];

            info.Name = name;
        }

        private unsafe void OnRegistryGlobal(void* data, wl_registry* wlRegistry, uint name, [MarshalAs(UnmanagedType.LPStr)] string @interface, uint version)
        {
            if (@interface == new string(LibWayland.kde_output_device_v2_interface->name))
            {
                var device = (kde_output_device_v2*)LibWayland.wl_registry_bind(wlRegistry, name, LibWayland.kde_output_device_v2_interface, Math.Min(version, 2));
                if (device == null)
                {
                    Console.WriteLine($"{nameof(LibWayland.wl_registry_bind)} failed");
                    return;
                }

                myDevices.Add((IntPtr)device);
                myModes.Add(IntPtr.Zero);
                myList.Add(new KdeWaylandMonitorInfo());
            }
        }

        private unsafe void OnRegistryGlobalRemove(void* data, wl_registry* wlRegistry, uint name)
        {
        }
    }
}