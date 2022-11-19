// using System.Runtime.InteropServices;
//
// namespace GetDisplayScale;
//
// public unsafe class Native
// {
//     const string libX11 = "libX11.so.6";
//     const string libX11Randr = "libXrandr.so.2";
//     private const string libGtk = "libgtk-3.so.0";
//     private const string libGdk = "libgdk-3.so.0";
//     
//     public struct SubpixelOrder
//     {
//         private ushort data;
//     }
//     
//     public struct Connection
//     {
//         private ushort data;
//     }
//
//     public struct Rotation
//     {
//         private ushort data;
//     }
//
//     public struct XFixed
//     {
//         private int data;
//
//         public static implicit operator double(XFixed value) => value.data / 65536.0;
//         public static implicit operator float(XFixed value) => value.data / 65536.0f;
//     }
//
//     public struct XTransform 
//     {
//         public XFixed matrix00;
//         public XFixed matrix01;
//         public XFixed matrix02;
//         public XFixed matrix10;
//         public XFixed matrix11;
//         public XFixed matrix12;
//         public XFixed matrix20;
//         public XFixed matrix21;
//         public XFixed matrix22;
//     }
//     
//     public struct GContext
//     {
//         // typedef unsigned long XID;
//         private IntPtr data;
//     }
//     
//     public struct Pixmap
//     {
//         // typedef unsigned long XID;
//         private IntPtr data;
//     }
//     
//     public struct Font
//     {
//         // typedef unsigned long XID;
//         private IntPtr data;
//     }
//
//     public struct Colormap
//     {
//         // typedef unsigned long XID;
//         private IntPtr data;
//     }
//
//     public struct Window
//     {
//         // typedef unsigned long XID;
//         private IntPtr data;
//     }
//     
//     public struct Time
//     {
//         //typedef unsigned long Time;
//         private IntPtr data;
//     }
//     
//     public struct Atom
//     { 
//         //typedef unsigned long Atom;	
//         private IntPtr data;
//     }
//     
//     public struct RROutput
//     {
//         // typedef unsigned long XID;
//         public IntPtr xid;
//     }
//     
//     public struct RRCrtc
//     {
//         // typedef unsigned long XID;
//         private IntPtr data;
//     }
//     
//     public struct XRRModeFlags
//     {
//         // typedef unsigned long XRRModeFlags;
//         private IntPtr data;
//     }
//     
//     public struct RRMode
//     {
//         // typedef unsigned long XID;
//         private IntPtr data;
//     }
//
//
//     public struct XGCValues 
//     {
//         public int function;    
//         public /* unsigned long */ IntPtr plane_mask; 
//         public /* unsigned long */ IntPtr foreground; 
//         public /* unsigned long */ IntPtr background; 
//         public int line_width;  
//         public int line_style;  
//         public int cap_style;   
//         public int join_style;  
//         public int fill_style;  
//         public int fill_rule;   
//         public int arc_mode;    
//         public Pixmap tile;     
//         public Pixmap stipple;  
//         public int ts_x_origin; 
//         public int ts_y_origin;
//         public Font font;       
//         public int subwindow_mode;      
//         public /* Bool */ int graphics_exposures; 
//         public int clip_x_origin;       
//         public int clip_y_origin;
//         public Pixmap clip_mask;        
//         public int dash_offset;         
//         public char dashes;
//     }
//     
//     public struct GC 
//     {
//         public /* XExtData* */ IntPtr ext_data; 
//         public GContext gid;
//         public /* Bool */ int rects;
//         public /* Bool */ int dashes;
//         public /* unsigned long */ IntPtr dirty; 
//         public XGCValues values;
//     }
//     
//     public struct Screen
//     {
//         public /* XExtData */ IntPtr ext_data;
//         public Display* display;
//         public Window root;
//         public int width;
//         public int height;
//         public int mwidth;
//         public int mheight;
//         public int ndepths;
//         public /* Depth* */ IntPtr depths;
//         public int root_depth;
//         public /* Visual* */ IntPtr root_visual;
//         public GC default_gc;
//         public Colormap cmap;
//         public /* unsigned long */ IntPtr white_pixel;
//         public /* unsigned long */ IntPtr black_pixel;
//         public int max_maps;
//         public int min_maps;
//         public int backing_store;
//         public /* Bool */ int save_unders;
//         public int root_input_mask;
//     }
//     
//     public struct Display
//     {
//         public /* XExtData* */ IntPtr ext_data;
//         public Display* next;
//         public int fd;
//         public int @lock;
//         public int proto_major_version;
//         public int proto_minor_version;
//         public byte* vendor;
//         public /* unsigned long */ IntPtr resource_base;
//         public /* unsigned long */ IntPtr resource_mask;
//         public /* unsigned long */ IntPtr resource_id;
//         public /* unsigned long */ IntPtr resource_shift;
//         public IntPtr resource_alloc;
//         public int byte_order;
//         public int bitmap_unit;
//         public int bitmap_pad;
//         public int bitmap_bit_order;
//         public int nformats;
//         public /* ScreenFormat* */ IntPtr pixmap_format;
//         public int vnumber;
//         public int release;
//
//         public /* _XSQEvent* */ IntPtr head;
//         public /* _XSQEvent* */ IntPtr tail;
//         public int qlen;
//         public int last_request_read;
//         public int request;
//         public byte* last_req;
//         public byte* buffer;
//         public byte* bufptr;
//         public byte* bufmax;
//         public uint max_request_size;
//
//         public /* _XrmHashBucketRec* */ IntPtr db;
//         public IntPtr synchandler;
//         public byte* display_name;
//         public int default_screen;
//         public int nscreens;
//         public Screen* screens;
//         public int motion_buffer;
//         public Window current;
//         public int min_keycode;
//         public int max_keycode;
//         public /* KeySym* */ IntPtr keysyms;
//         public /* XModifierKeymap* */ IntPtr modifiermap;
//         public int keysyms_per_keycode;
//         public byte* xdefaults;
//         public byte* scratch_buffer;
//         public int scratch_length;
//         public int ext_number;
//         public /* _XExtension* */ IntPtr ext_procs;
//
//         //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
//         //public IntPtr[] event_vec;
//
//         //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
//         //public IntPtr[] wire_vec;
//     }
//     
//     [DllImport(libX11)]
//     public static extern Display* XOpenDisplay([MarshalAs(UnmanagedType.LPStr)] string display);
//
//     [DllImport(libX11)]
//     public static extern Window XDefaultRootWindow(Display* display);
//
//     public struct XRRMonitorInfo 
//     {
//         public Atom name;
//         public /* Bool */ int primary;
//         public /* Bool */ int automatic;
//         public int noutput;
//         public int x;
//         public int y;
//         public int width;
//         public int height;
//         public int mwidth;
//         public int mheight;
//         public RROutput *outputs;
//     }
//
//     [DllImport(libX11Randr)]
//     public static extern XRRMonitorInfo* XRRGetMonitors(Display* dpy, Window window, bool getActive, out int nMonitors);
//     
//     public struct XRRModeInfo 
//     {
//         public RRMode		id;
//         public uint	width;
//         public uint	height;
//         public /* unsigned long */ IntPtr	dotClock;
//         public uint	hSyncStart;
//         public uint	hSyncEnd;
//         public uint	hTotal;
//         public uint	hSkew;
//         public uint	vSyncStart;
//         public uint	vSyncEnd;
//         public uint	vTotal;
//         public byte		*name;
//         public uint	nameLength;
//         public XRRModeFlags	modeFlags;
//     } 
//     
//     public struct XRRScreenResources 
//     {
//         public Time	timestamp;
//         public Time	configTimestamp;
//         public int		ncrtc;
//         public RRCrtc	*crtcs;
//         public int		noutput;
//         public RROutput	*outputs;
//         public int		nmode;
//         public XRRModeInfo	*modes;
//     }
//
//     [DllImport(libX11Randr)]
//     public static extern XRRScreenResources* XRRGetScreenResources (Display* dpy, Window window);
//     
//     
//     [DllImport(libX11Randr)]
//     public static extern void XRRFreeScreenResources (XRRScreenResources *resources);
//
//     public struct XRROutputInfo 
//     {
//         public Time	    timestamp;
//         public RRCrtc	    crtc;
//         public byte	    *name;
//         public int		    nameLen;
//         public /* unsigned long */ IntPtr mm_width;
//         public /* unsigned long */ IntPtr mm_height;
//         public Connection	    connection;
//         public SubpixelOrder   subpixel_order;
//         public int		    ncrtc;
//         public RRCrtc	    *crtcs;
//         public int		    nclone;
//         public RROutput	    *clones;
//         public int		    nmode;
//         public int		    npreferred;
//         public RRMode	    *modes;
//     }
//
//     
//     [DllImport(libX11Randr)]
//     public static extern XRROutputInfo* XRRGetOutputInfo (Display *dpy, XRRScreenResources *resources, RROutput output);
//
//     
//     [DllImport(libX11Randr)]
//     public static extern void XRRFreeOutputInfo (XRROutputInfo *outputInfo);
//     
//     [DllImport(libX11)]
//     public static extern IntPtr XGetAtomName(Display* display, Atom atom);
//
//     [DllImport(libX11)]
//     public static extern void XFree(void* data);
//
//     [DllImport(libX11)]
//     public static extern void XFree(IntPtr data);
//
//     public struct XRRCrtcInfo 
//     {
//         Time	    timestamp;
//         int		    x, y;
//         uint    width, height;
//         RRMode	    mode;
//         Rotation	    rotation;
//         int		    noutput;
//         RROutput	    *outputs;
//         Rotation	    rotations;
//         int		    npossible;
//         RROutput	    *possible;
//     }
//
//     
//     [DllImport(libX11Randr)]
//     public static extern XRRCrtcInfo* XRRGetCrtcInfo (Display *dpy, XRRScreenResources *resources, RRCrtc crtc);
//
//     
//     [DllImport(libX11Randr)]
//     public static extern void XRRFreeCrtcInfo (XRRCrtcInfo *crtcInfo);
//     
//     public struct XRRCrtcTransformAttributes {
//         public XTransform	pendingTransform;
//         public byte* pendingFilter;
//         public int		pendingNparams;
//         public XFixed	*pendingParams;
//         public XTransform	currentTransform;
//         public byte	*currentFilter;
//         public int		currentNparams;
//         public XFixed	*currentParams;
//     }
//
//     
//     [DllImport(libX11Randr)]
//     public static extern int
//         XRRGetCrtcTransform (Display	*dpy,
//             RRCrtc	crtc,
//             out XRRCrtcTransformAttributes* attributes);
//
//     
//     [DllImport(libGtk)]
//     public static extern bool gtk_init_check(int argc, IntPtr argv);
//
//     public struct GdkDisplay
//     {
//     }
//     
//     public struct GdkScreen
//     {
//     }
//     
//     public struct GdkMonitor
//     {
//     }
//
//     [DllImport(libGdk)]
//     public static extern GdkDisplay* gdk_display_get_default();
//
//     [DllImport(libGdk)]
//     public static extern int gdk_display_get_n_screens (GdkDisplay* display);
//         
//     [DllImport(libGdk)]
//     public static extern  GdkScreen* gdk_display_get_default_screen (GdkDisplay *display);
//
//     [DllImport(libGdk)]
//     public static extern int gdk_display_get_n_monitors (GdkDisplay *display);
//     
//     [DllImport(libGdk)]
//     public static extern IntPtr gdk_x11_screen_get_monitor_output (GdkScreen* screen, int monitor_num);
//     
//     [DllImport(libGdk)]
//     public static extern GdkMonitor* gdk_display_get_monitor (GdkDisplay* display, int monitorNum);
//     
//     [DllImport(libGdk)]
//     public static extern int gdk_monitor_get_scale_factor(GdkMonitor* monitor); 
//
//     [DllImport(libX11)]
//     [return: MarshalAs(UnmanagedType.LPStr)]
//     public static extern string XResourceManagerString(Display* display);
//
//
//
//     public struct GSettingsSchemaSource
//     {
//     }
//     
//     public struct GSettings
//     {
//     }
//     
//     [DllImport(libGdk)]
//     public static extern GSettingsSchemaSource* g_settings_schema_source_get_default ();
//
//     public struct GSettingsSchema
//     {
//         
//     }
//     
//     
//     [DllImport(libGdk)]
//     public static extern GSettingsSchema* g_settings_schema_source_lookup (GSettingsSchemaSource* source,
//         [MarshalAs(UnmanagedType.LPStr)] string schema_id,
//         bool recursive);
//     
//     [DllImport(libGdk)]
//     public static extern GSettings* g_settings_new([MarshalAs(UnmanagedType.LPStr)] string schema_id);
//         
//     [DllImport(libGdk)]
//     public static extern IntPtr g_settings_get_strv (GSettings* settings, [MarshalAs(UnmanagedType.LPStr)]  string key);
//
//     public struct GdkRectangle 
//     {
//         public int x;
//         public int y;
//         public int width;
//         public int height;
//     }
//     
//     [DllImport(libGdk)]
//     public static extern void  gdk_monitor_get_geometry (GdkMonitor* monitor, out GdkRectangle geometry);
//     
//     
//     [DllImport(libGdk)]
//     public static extern bool gdk_monitor_is_primary (GdkMonitor *monitor);
//
// }