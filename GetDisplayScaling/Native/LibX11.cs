using System.Runtime.InteropServices;

namespace GetDisplayScaling.Native;

public static unsafe class LibX11
{
    public const string Lib = "libX11.so.6";
    public static bool Exists => LibDl.CanBeLoaded(Lib);
    
    // XCloseDisplay may fail with "free(): double free detected in tcache 2", so for all read-only actions use one static handle instead opening display everywhere
    private static readonly Lazy<IntPtr> ourXDisplay = new (() => (IntPtr)XOpenDisplay(null));

    public static Display* Display => (Display*)ourXDisplay.Value;
    
    [DllImport(Lib)]
    private static extern Display* XOpenDisplay([MarshalAs(UnmanagedType.LPStr)] string display);

    [DllImport(Lib)]
    public static extern int XCloseDisplay(Display* display);

    // Must call XFree, so cannot marshal as string
    [DllImport(Lib)]
    public static extern sbyte* XResourceManagerString(Display* display);

    // Must call XFree, so cannot marshal as string
    [DllImport(Lib)]
    public static extern sbyte* XGetAtomName(Display* display, Atom atom);

    [DllImport(Lib)]
    public static extern int XFree(void* data);
    
    [DllImport(Lib)]
    public static extern Window XDefaultRootWindow(Display* display);
}

public struct Display
{
    // We use only pointers for strict typing
}

public struct Window
{
    private IntPtr data; // XID;
}

public struct SubpixelOrder
{
    private ushort data;
}
    
public struct Connection
{
    private ushort data;
}

public struct Time
{
    private IntPtr data;
}

public struct Atom
{ 
    private IntPtr data;
}

public struct XFixed
{
    private int data;

    public static implicit operator double(XFixed value) => value.data / 65536.0;
    public static implicit operator float(XFixed value) => value.data / 65536.0f;
}

public struct XTransform 
{
    public XFixed M00;
    public XFixed M01;
    public XFixed M02;
    public XFixed M10;
    public XFixed M11;
    public XFixed M12;
    public XFixed M20;
    public XFixed M21;
    public XFixed M22;
}