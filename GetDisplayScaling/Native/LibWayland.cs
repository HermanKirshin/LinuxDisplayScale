using System.Runtime.InteropServices;

namespace GetDisplayScaling.Native;

public unsafe class LibWayland
{
    public const string Lib = "libwayland-client.so.0";
    public static bool Exists => LibDl.CanBeLoaded(Lib);

    private static WaylandStaticInterfaces staticInterfaces = new();
    public static wl_interface* wl_registry_interface => staticInterfaces.wl_registry_interface;
    public static wl_interface* wl_output_interface => staticInterfaces.wl_output_interface;
    public static wl_interface* zxdg_output_v1_interface => staticInterfaces.zxdg_output_v1_interface;
    public static wl_interface* zxdg_output_manager_v1_interface => staticInterfaces.zxdg_output_manager_v1_interface;
    public static wl_interface* kde_output_device_v2_interface => staticInterfaces.kde_output_device_v2_interface;
    public static wl_interface* kde_output_device_mode_v2_interface => staticInterfaces.kde_output_device_mode_v2_interface;
    
    [DllImport(Lib)]
    public static extern wl_display* wl_display_connect([MarshalAs(UnmanagedType.LPStr)] string name);
    
    [DllImport(Lib)]
    public static extern void wl_display_disconnect(wl_display* display);
    
    [DllImport(Lib)]
    private static extern uint wl_proxy_get_version(wl_proxy* proxy);

    [DllImport(Lib)]
    private static extern wl_proxy* wl_proxy_marshal_constructor(wl_proxy *proxy, uint opcode, wl_interface *@interface, void* @params);
    
    [DllImport(Lib, EntryPoint = "wl_proxy_marshal_flags")]
    private static extern wl_proxy* wl_proxy_marshal_flags_0(wl_proxy* proxy, uint opcode, wl_interface* @interface, uint version, uint flags, void* _);
    
    [DllImport(Lib, EntryPoint = "wl_proxy_marshal_flags")]
    private static extern wl_proxy* wl_proxy_marshal_flags_1(wl_proxy* proxy, uint opcode, wl_interface* @interface, uint version, uint flags, void* param0, void* _);
    
    [DllImport(Lib, EntryPoint = "wl_proxy_marshal_flags")]
    private static extern wl_proxy* wl_proxy_marshal_flags_2(wl_proxy* proxy, uint opcode, wl_interface* @interface, uint version, uint flags, void* param0, void* param1, void* _);
    
    [DllImport(Lib, EntryPoint = "wl_proxy_marshal_flags")]
    private static extern wl_proxy* wl_proxy_marshal_flags_3(wl_proxy* proxy, uint opcode, wl_interface* @interface, uint version, uint flags, void* param0, void* param1, void* param2, void* _);
    
    [DllImport(Lib, EntryPoint = "wl_proxy_marshal_flags")]
    private static extern wl_proxy* wl_proxy_marshal_flags_4(wl_proxy* proxy, uint opcode, wl_interface* @interface, uint version, uint flags, void* param0, void* param1, void* param2, void* param3, void* _);

    [DllImport(Lib)]
    private static extern int wl_proxy_add_listener(wl_proxy* proxy, void* implementation, void *data);
    
    [DllImport(Lib)]
    public static extern int wl_display_roundtrip(wl_display* display);

    [DllImport(Lib)]
    public static extern void wl_proxy_destroy(wl_proxy* proxy);
    
    [DllImport(Lib)]
    public static extern uint  wl_proxy_get_id(wl_proxy* proxy);

    public const int WL_OUTPUT_MODE_CURRENT = 1;
    
    public const int WL_MARSHAL_FLAG_DESTROY = (1 << 0);
    
    public const int WL_DISPLAY_GET_REGISTRY = 1;
    
    public static wl_registry* wl_display_get_registry(wl_display* wl_display) => (wl_registry*)wl_proxy_marshal_flags_1((wl_proxy*)wl_display,
        WL_DISPLAY_GET_REGISTRY, wl_registry_interface, wl_proxy_get_version((wl_proxy*)wl_display), 0, null, null);
    
    public static int wl_registry_add_listener(wl_registry* wl_registry, wl_registry_listener* listener, void *data) =>
        wl_proxy_add_listener((wl_proxy*)wl_registry, listener, data);

    public const int WL_REGISTRY_BIND = 0;
    
    public static void* wl_registry_bind(wl_registry *wl_registry, uint name, wl_interface* @interface, uint version) => wl_proxy_marshal_flags_4((wl_proxy*)wl_registry,
        WL_REGISTRY_BIND, @interface, version, 0, (void*)name, @interface->name, (void*)version, null, null);
    
    public static int wl_output_add_listener(wl_output* wl_output, wl_output_listener* listener, void *data) => wl_proxy_add_listener((wl_proxy*) wl_output, listener, data);

    public const int ZXDG_OUTPUT_MANAGER_V1_GET_XDG_OUTPUT = 1;
    
    public static zxdg_output_v1* zxdg_output_manager_v1_get_xdg_output(zxdg_output_manager_v1* zxdg_output_manager_v1, wl_output* output) =>
        (zxdg_output_v1*)wl_proxy_marshal_flags_3((wl_proxy*)zxdg_output_manager_v1,
        ZXDG_OUTPUT_MANAGER_V1_GET_XDG_OUTPUT, zxdg_output_v1_interface, wl_proxy_get_version((wl_proxy*)zxdg_output_manager_v1), 0, null, output, null, null);

    public static int zxdg_output_v1_add_listener(zxdg_output_v1* zxdg_output_v1, zxdg_output_v1_listener* listener, void *data) =>
        wl_proxy_add_listener((wl_proxy*)zxdg_output_v1, listener, data);

    public const int ZXDG_OUTPUT_MANAGER_V1_DESTROY = 0;
    
    public static void zxdg_output_manager_v1_destroy(zxdg_output_manager_v1* zxdg_output_manager_v1) =>
        wl_proxy_marshal_flags_0((wl_proxy*)zxdg_output_manager_v1,
        ZXDG_OUTPUT_MANAGER_V1_DESTROY, null, wl_proxy_get_version((wl_proxy*)zxdg_output_manager_v1), WL_MARSHAL_FLAG_DESTROY, null);
    
    public const int ZXDG_OUTPUT_V1_DESTROY = 0;
    
    public static void zxdg_output_v1_destroy(zxdg_output_v1* zxdg_output_v1) =>
        wl_proxy_marshal_flags_0((wl_proxy*)zxdg_output_v1,
        ZXDG_OUTPUT_V1_DESTROY, null, wl_proxy_get_version((wl_proxy*)zxdg_output_v1), WL_MARSHAL_FLAG_DESTROY, null);

    public static void wl_output_destroy(wl_output* wl_output) =>
        wl_proxy_destroy((wl_proxy*)wl_output);
    
    public static void wl_registry_destroy(wl_registry* wl_registry) =>
        wl_proxy_destroy((wl_proxy*)wl_registry);
    
    public static int kde_output_device_v2_add_listener(kde_output_device_v2* kde_output_device_v2, kde_output_device_v2_listener* listener, void *data) =>
        wl_proxy_add_listener((wl_proxy*)kde_output_device_v2, listener, data);
    
    public static void kde_output_device_v2_destroy(kde_output_device_v2* kde_output_device_v2) =>
        wl_proxy_destroy((wl_proxy*)kde_output_device_v2);
    
    public static int kde_output_device_mode_v2_add_listener(kde_output_device_mode_v2* kde_output_device_mode_v2, kde_output_device_mode_v2_listener* listener, void *data) =>
        wl_proxy_add_listener((wl_proxy*)kde_output_device_mode_v2, listener, data);
    
    public static void kde_output_device_mode_v2_destroy(kde_output_device_mode_v2* kde_output_device_mode_v2) =>
        wl_proxy_destroy((wl_proxy*)kde_output_device_mode_v2);
}

public struct wl_display
{
}
    
public struct wl_output
{
}
    
public struct wl_proxy
{
}

public struct wl_registry
{
}

public struct kde_output_device_v2
{
    
}

public struct kde_output_device_mode_v2
{
}

public struct wl_fixed_t
{
    private int data;
    
    public static implicit operator double(wl_fixed_t value) => value.data / 256.0;
    public static implicit operator float(wl_fixed_t value) => value.data / 256.0f;
}

public unsafe struct kde_output_device_mode_v2_listener 
{
    public delegate void size_callback(void *data, kde_output_device_mode_v2* kde_output_device_mode_v2, int width, int height);
    public delegate void refresh_callback(void *data, kde_output_device_mode_v2 *kde_output_device_mode_v2, int refresh);
    public delegate void preferred_callback(void *data, kde_output_device_mode_v2 *kde_output_device_mode_v2);
    public delegate void removed_callback(void *data, kde_output_device_mode_v2 *kde_output_device_mode_v2);

    public IntPtr size;
    public IntPtr refresh;
    public IntPtr preferred;
    public IntPtr removed;
    
    public kde_output_device_mode_v2_listener(size_callback size, refresh_callback refresh, preferred_callback preferred, removed_callback removed, out Action keepAlive)
    {
        this.size = size == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(size);
        this.refresh = refresh == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(refresh);
        this.preferred = preferred == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(preferred);
        this.removed = removed == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(removed);
        keepAlive = () =>
        {
            GC.KeepAlive(size);
            GC.KeepAlive(refresh);
            GC.KeepAlive(preferred);
            GC.KeepAlive(removed);
        };
    }
}

public unsafe struct kde_output_device_v2_listener
{
    public delegate void geometry_callback(void* data, kde_output_device_v2* kde_output_device_v2, int x, int y, int physical_width, int physical_height, int subpixel, [MarshalAs(UnmanagedType.LPStr)] string make, [MarshalAs(UnmanagedType.LPStr)] string model, int transform);
    public delegate void current_mode_callback(void* data, kde_output_device_v2* kde_output_device_v2, kde_output_device_mode_v2* mode);
    public delegate void mode_callback(void* data, kde_output_device_v2* kde_output_device_v2, kde_output_device_mode_v2* mode);
    public delegate void done_callback(void* data, kde_output_device_v2* kde_output_device_v2);
    public delegate void scale_callback(void* data, kde_output_device_v2* kde_output_device_v2, wl_fixed_t factor);
    public delegate void edid_callback(void* data, kde_output_device_v2* kde_output_device_v2, [MarshalAs(UnmanagedType.LPStr)] string raw);
    public delegate void enabled_callback(void* data, kde_output_device_v2* kde_output_device_v2, int enabled);
    public delegate void uuid_callback(void* data, kde_output_device_v2* kde_output_device_v2, [MarshalAs(UnmanagedType.LPStr)] string uuid);
    public delegate void serial_number_callback(void* data, kde_output_device_v2* kde_output_device_v2, [MarshalAs(UnmanagedType.LPStr)] string serialNumber);
    public delegate void eisa_id_callback(void* data, kde_output_device_v2* kde_output_device_v2, [MarshalAs(UnmanagedType.LPStr)] string eisaId);
    public delegate void capabilities_callback(void* data, kde_output_device_v2* kde_output_device_v2, uint flags);
    public delegate void overscan_callback(void* data, kde_output_device_v2* kde_output_device_v2, uint overscan);
    public delegate void vrr_policy_callback(void* data, kde_output_device_v2* kde_output_device_v2, uint vrr_policy);
    public delegate void rgb_range_callback(void* data, kde_output_device_v2* kde_output_device_v2, uint rgb_range);
    public delegate void name_callback(void* data, kde_output_device_v2* kde_output_device_v2, [MarshalAs(UnmanagedType.LPStr)] string name);

    public IntPtr geometry;
    public IntPtr current_mode;
    public IntPtr mode;
    public IntPtr done;
    public IntPtr scale;
    public IntPtr edid;
    public IntPtr enabled;
    public IntPtr uuid;
    public IntPtr serial_number;
    public IntPtr eisa_id;
    public IntPtr capabilities;
    public IntPtr overscan;
    public IntPtr vrr_policy;
    public IntPtr rgb_range;
    public IntPtr name;

    public kde_output_device_v2_listener(geometry_callback geometry, current_mode_callback current_mode, mode_callback mode, done_callback done, scale_callback scale, edid_callback edid, enabled_callback enabled, uuid_callback uuid, serial_number_callback serial_number, eisa_id_callback eisa_id, capabilities_callback capabilities, overscan_callback overscan, vrr_policy_callback vrr_policy, rgb_range_callback rgb_range, name_callback name, out Action keepAlive)
    {
        this.geometry = geometry == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(geometry);
        this.current_mode = current_mode == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(current_mode);
        this.mode = mode == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(mode);
        this.done = done == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(done);
        this.scale = scale == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(scale);
        this.edid = edid == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(edid);
        this.enabled = enabled == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(enabled);
        this.uuid = uuid == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(uuid);
        this.serial_number = serial_number == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(serial_number);
        this.eisa_id = eisa_id == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(eisa_id);
        this.capabilities = capabilities == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(capabilities);
        this.overscan = overscan == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(overscan);
        this.vrr_policy = vrr_policy == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(vrr_policy);
        this.rgb_range = rgb_range == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(rgb_range);
        this.name = name == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(name);
        keepAlive = () =>
        {
            GC.KeepAlive(geometry);
            GC.KeepAlive(current_mode);
            GC.KeepAlive(mode);
            GC.KeepAlive(done);
            GC.KeepAlive(scale);
            GC.KeepAlive(edid);
            GC.KeepAlive(enabled);
            GC.KeepAlive(uuid);
            GC.KeepAlive(serial_number);
            GC.KeepAlive(eisa_id);
            GC.KeepAlive(capabilities);
            GC.KeepAlive(overscan);
            GC.KeepAlive(vrr_policy);
            GC.KeepAlive(rgb_range);
            GC.KeepAlive(name);
        };
    }
}

public unsafe struct wl_output_listener
{
    public delegate void geometry_callback(void* data, wl_output* wl_output, int x, int y, int physical_width, int physical_height, int subpixel, [MarshalAs(UnmanagedType.LPStr)] string make, [MarshalAs(UnmanagedType.LPStr)] string model, int transform);
    public delegate void mode_callback(void* data, wl_output* wl_output, uint flags, int width, int height, int refresh);
    public delegate void done_callback(void* data, wl_output* wl_output);
    public delegate void scale_callback(void* data, wl_output* wl_output, int factor);
    
    public IntPtr geometry;
    public IntPtr mode;
    public IntPtr done;
    public IntPtr scale;

    public wl_output_listener(geometry_callback geometry, mode_callback mode, done_callback done, scale_callback scale, out Action keepAlive)
    {
        this.geometry = geometry == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(geometry);
        this.mode = mode == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(mode);
        this.done = done == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(done);
        this.scale = scale == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(scale);
        keepAlive = () =>
        {
            GC.KeepAlive(geometry);
            GC.KeepAlive(mode);
            GC.KeepAlive(done);
            GC.KeepAlive(scale);
        };
    }
}
        
public unsafe struct wl_registry_listener
{
    public delegate void global_callback(void *data, wl_registry *wl_registry, uint name, [MarshalAs(UnmanagedType.LPStr)] string @interface, uint version);
    public delegate void global_revove_callback(void *data, wl_registry *wl_registry, uint name);
    
    public IntPtr global; 
    public IntPtr global_remove;
    
    public wl_registry_listener(global_callback global, global_revove_callback global_remove, out Action keepAlive)
    {
        this.global = global == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(global);
        this.global_remove = global_remove == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(global_remove);
        keepAlive = () =>
        {
            GC.KeepAlive(global);
            GC.KeepAlive(global_remove);
        };
    }
};

public unsafe struct zxdg_output_v1_listener
{
    public delegate void logical_position_callback(void *data, zxdg_output_v1 *zxdg_output_v1, int x, int y);
    public delegate void logical_size_callback(void *data, zxdg_output_v1 *zxdg_output_v1, int width, int height);
    public delegate void done_callback(void *data, zxdg_output_v1 *zxdg_output_v1);
    public delegate void name_callback(void *data,zxdg_output_v1 *zxdg_output_v1, [MarshalAs(UnmanagedType.LPStr)] string  name);
    public delegate void description_callback(void *data, zxdg_output_v1 *zxdg_output_v1, [MarshalAs(UnmanagedType.LPStr)] string description);
    
    public IntPtr logical_position;
    public IntPtr logical_size;
    public IntPtr done;
    public IntPtr name;
    public IntPtr description; 
    
    public zxdg_output_v1_listener(logical_position_callback logical_position, logical_size_callback logical_size, done_callback done, name_callback name, description_callback description, out Action keepAlive)
    {
        this.logical_position = logical_position == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(logical_position);
        this.logical_size = logical_size == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(logical_size);
        this.done = done == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(done);
        this.name = name == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(name);
        this.description = description == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(description);
        keepAlive = () =>
        {
            GC.KeepAlive(logical_position);
            GC.KeepAlive(logical_size);
            GC.KeepAlive(done);
            GC.KeepAlive(name);
            GC.KeepAlive(description); 
        };
    }
};

public struct zxdg_output_v1
{
        
}

public struct zxdg_output_manager_v1
{
    
}

public unsafe struct wl_message
{
    public sbyte *name;
    public sbyte *signature;
    public wl_interface **types;
}

public unsafe struct wl_interface
{
    public sbyte* name;
    public int version;
    public int method_count;
    public wl_message* methods;
    public int event_count;
    public wl_message* events;
}

// native static variables
public sealed unsafe class WaylandStaticInterfaces : IDisposable
{
    private IntPtr libWayland;
    private readonly Dictionary<string, IntPtr> allocatedStrings = new();
    private readonly List<IntPtr> allocations = new();
    
    public wl_interface* wl_registry_interface { get; }
    public wl_interface* wl_output_interface { get; }
    public wl_interface* zxdg_output_manager_v1_interface { get; }
    public wl_interface* zxdg_output_v1_interface { get; }
    public wl_interface* kde_output_device_v2_interface { get; }
    public wl_interface* kde_output_device_mode_v2_interface { get; }

    public WaylandStaticInterfaces()
    {
        libWayland = LibDl.dlopen(LibWayland.Lib, LibDl.RTLD_LAZY);
        if (libWayland == IntPtr.Zero)
            return;

        wl_registry_interface = (wl_interface*)LibDl.dlsym(libWayland, "wl_registry_interface");
        wl_output_interface = (wl_interface*)LibDl.dlsym(libWayland, "wl_output_interface");

        // port from generated xdg-output-unstable-v1-client-protocol.c

        zxdg_output_manager_v1_interface = (wl_interface*)Marshal.AllocHGlobal(Marshal.SizeOf<wl_interface>());
        zxdg_output_v1_interface = (wl_interface*)Marshal.AllocHGlobal(Marshal.SizeOf<wl_interface>());

        var xdgTypes = (wl_interface**)AllocateArray(
            IntPtr.Zero,
            IntPtr.Zero,
            (IntPtr)zxdg_output_v1_interface,
            (IntPtr)wl_output_interface);

        var xdgOutputManagerRequests = (wl_message*)AllocateArray(
            new wl_message
            {
                name = GetStringPtr("destroy"),
                signature = GetStringPtr(""),
                types = xdgTypes + 0
            },
            new wl_message
            {
                name = GetStringPtr("get_xdg_output"),
                signature = GetStringPtr("no"),
                types = xdgTypes + 2
            });

        var xdgOutputRequests = (wl_message*)AllocateArray(
            new Native.wl_message
            {
                name = GetStringPtr("destroy"),
                signature = GetStringPtr(""),
                types = xdgTypes + 0
            });

        var xdgOutputEvents = (wl_message*)AllocateArray(
            new wl_message
            {
                name = GetStringPtr("logical_position"),
                signature = GetStringPtr("ii"),
                types = xdgTypes + 0
            },
            new wl_message
            {
                name = GetStringPtr("logical_size"),
                signature = GetStringPtr("ii"),
                types = xdgTypes + 0
            },
            new wl_message
            {
                name = GetStringPtr("done"),
                signature = GetStringPtr(""),
                types = xdgTypes + 0
            },
            new wl_message
            {
                name = GetStringPtr("name"),
                signature = GetStringPtr("2s"),
                types = xdgTypes + 0
            },
            new wl_message
            {
                name = GetStringPtr("description"),
                signature = GetStringPtr("2s"),
                types = xdgTypes + 0
            });

        zxdg_output_manager_v1_interface->name = GetStringPtr("zxdg_output_manager_v1");
        zxdg_output_manager_v1_interface->version = 3;
        zxdg_output_manager_v1_interface->method_count = 2;
        zxdg_output_manager_v1_interface->methods = xdgOutputManagerRequests;
        zxdg_output_manager_v1_interface->event_count = 0;
        zxdg_output_manager_v1_interface->events = null;

        zxdg_output_v1_interface->name = GetStringPtr("zxdg_output_v1");
        zxdg_output_v1_interface->version = 3;
        zxdg_output_v1_interface->method_count = 1;
        zxdg_output_v1_interface->methods = xdgOutputRequests;
        zxdg_output_v1_interface->event_count = 5;
        zxdg_output_v1_interface->events = xdgOutputEvents;

        // port from kde_output_device_v2-unstable-v2-protocol.c

        kde_output_device_v2_interface = (wl_interface*)Marshal.AllocHGlobal(Marshal.SizeOf<wl_interface>());
        kde_output_device_mode_v2_interface = (wl_interface*)Marshal.AllocHGlobal(Marshal.SizeOf<wl_interface>());

        var kdeTypes = (wl_interface**)AllocateArray(
            IntPtr.Zero,
            IntPtr.Zero,
            IntPtr.Zero,
            IntPtr.Zero,
            IntPtr.Zero,
            IntPtr.Zero,
            IntPtr.Zero,
            IntPtr.Zero,
            (IntPtr)kde_output_device_mode_v2_interface,
            (IntPtr)kde_output_device_mode_v2_interface);

        var kdeOutputDeviceEvents = (wl_message*)AllocateArray(
            new wl_message
            {
                name = GetStringPtr("geometry"),
                signature = GetStringPtr("iiiiissi"),
                types = kdeTypes + 0
            },
            new wl_message
            {
                name = GetStringPtr("current_mode"),
                signature = GetStringPtr("o"),
                types = kdeTypes + 8
            },
            new wl_message
            {
                name = GetStringPtr("mode"),
                signature = GetStringPtr("n"),
                types = kdeTypes + 9
            },
            new wl_message
            {
                name = GetStringPtr("done"),
                signature = GetStringPtr(""),
                types = kdeTypes + 0
            },
            new wl_message
            {
                name = GetStringPtr("scale"),
                signature = GetStringPtr("f"),
                types = kdeTypes + 0
            },
            new wl_message
            {
                name = GetStringPtr("edid"),
                signature = GetStringPtr("s"),
                types = kdeTypes + 0
            },
            new wl_message
            {
                name = GetStringPtr("enabled"),
                signature = GetStringPtr("i"),
                types = kdeTypes + 0
            },
            new wl_message
            {
                name = GetStringPtr("uuid"),
                signature = GetStringPtr("s"),
                types = kdeTypes + 0
            },
            new wl_message
            {
                name = GetStringPtr("serial_number"),
                signature = GetStringPtr("s"),
                types = kdeTypes + 0
            },
            new wl_message
            {
                name = GetStringPtr("eisa_id"),
                signature = GetStringPtr("s"),
                types = kdeTypes + 0
            },
            new wl_message
            {
                name = GetStringPtr("capabilities"),
                signature = GetStringPtr("u"),
                types = kdeTypes + 0
            },
            new wl_message
            {
                name = GetStringPtr("overscan"),
                signature = GetStringPtr("u"),
                types = kdeTypes + 0
            },
            new wl_message
            {
                name = GetStringPtr("vrr_policy"),
                signature = GetStringPtr("u"),
                types = kdeTypes + 0
            },
            new wl_message
            {
                name = GetStringPtr("rgb_range"),
                signature = GetStringPtr("u"),
                types = kdeTypes + 0
            },
            new wl_message
            {
                name = GetStringPtr("name"),
                signature = GetStringPtr("2s"),
                types = kdeTypes + 0
            });

        var kdeOutputDeviceModeEvents = (wl_message*)AllocateArray(
            new wl_message
            {
                name = GetStringPtr("size"),
                signature = GetStringPtr("ii"),
                types = kdeTypes + 0
            },
            new wl_message
            {
                name = GetStringPtr("refresh"),
                signature = GetStringPtr("i"),
                types = kdeTypes + 0
            },
            new wl_message
            {
                name = GetStringPtr("preferred"),
                signature = GetStringPtr(""),
                types = kdeTypes + 0
            },
            new wl_message
            {
                name = GetStringPtr("removed"),
                signature = GetStringPtr(""),
                types = kdeTypes + 0
            });

        kde_output_device_v2_interface->name = GetStringPtr("kde_output_device_v2");
        kde_output_device_v2_interface->version = 2;
        kde_output_device_v2_interface->method_count = 0;
        kde_output_device_v2_interface->methods = null;
        kde_output_device_v2_interface->event_count = 15;
        kde_output_device_v2_interface->events = kdeOutputDeviceEvents;

        kde_output_device_mode_v2_interface->name = GetStringPtr("kde_output_device_mode_v2");
        kde_output_device_mode_v2_interface->version = 1;
        kde_output_device_mode_v2_interface->method_count = 0;
        kde_output_device_mode_v2_interface->methods = null;
        kde_output_device_mode_v2_interface->event_count = 4;
        kde_output_device_mode_v2_interface->events = kdeOutputDeviceModeEvents;
    }

    private void* AllocateArray<T>(params T[] values)
    {
        var result = Marshal.AllocHGlobal(Marshal.SizeOf<T>() * values.Length);
        allocations.Add(result);
        for (var i = 0; i < values.Length; i++)
            Marshal.StructureToPtr(values[i], result + Marshal.SizeOf<T>() * i, false);
        return (void*)result;
    }
    
    private sbyte* GetStringPtr(string text)
    {
        if (allocatedStrings.TryGetValue(text, out var pointer)) 
            return (sbyte*)pointer;
            
        pointer = Marshal.StringToHGlobalAnsi(text);
        allocatedStrings.Add(text, pointer);
        allocations.Add(pointer);
            
        return (sbyte*)pointer;
    }

    private void ReleaseUnmanagedResources()
    {
        allocations.ForEach(Marshal.FreeHGlobal);
        allocations.Clear();
        allocatedStrings.Clear();
        
        if (libWayland == IntPtr.Zero)
            return;
        LibDl.dlclose(libWayland);
        libWayland = IntPtr.Zero;
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~WaylandStaticInterfaces() =>
        ReleaseUnmanagedResources();
}