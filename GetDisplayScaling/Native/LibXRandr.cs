using System;
using System.Runtime.InteropServices;

namespace GetDisplayScaling.Native;

public static unsafe class LibXRandr
{
  public const string Lib = "libXrandr.so.2";
  public static bool Exists => LibDl.CanBeLoaded(Lib);

  [DllImport(Lib)]
  public static extern XRRMonitorInfo* XRRGetMonitors(Display* dpy, Window window, bool getActive, out int nMonitors);

  // Must call XRRFreeScreenResources
  [DllImport(Lib)]
  public static extern XRRScreenResources* XRRGetScreenResources(Display* dpy, Window window);

  // Must call XRRFreeOutputInfo
  [DllImport(Lib)]
  public static extern XRROutputInfo* XRRGetOutputInfo(Display* dpy, XRRScreenResources* resources, RROutput output);

  [DllImport(Lib)]
  public static extern int XRRGetCrtcTransform(Display* dpy, RRCrtc crtc, out XRRCrtcTransformAttributes* attributes);

  [DllImport(Lib)]
  public static extern Atom* XRRListOutputProperties(Display* dpy, RROutput output, out int nprop);

  [DllImport(Lib)]
  public static extern void XRRFreeOutputInfo(XRROutputInfo* outputInfo);

  [DllImport(Lib)]
  public static extern void XRRFreeScreenResources(XRRScreenResources* resources);
}

public struct RRCrtc
{
  private IntPtr data; // XID;
}
    
public struct XRRModeFlags
{
  private IntPtr data;
}
    
public struct RRMode
{
  private IntPtr data; // XID;
}

public struct RROutput
{
  public IntPtr xid; // XID;
}

public unsafe struct XRRModeInfo 
{
  public RRMode		id;
  public uint	width;
  public uint	height;
  public IntPtr	dotClock;
  public uint	hSyncStart;
  public uint	hSyncEnd;
  public uint	hTotal;
  public uint	hSkew;
  public uint	vSyncStart;
  public uint	vSyncEnd;
  public uint	vTotal;
  public byte		*name;
  public uint	nameLength;
  public XRRModeFlags	modeFlags;
} 

public unsafe struct XRRScreenResources 
{
  public Time	timestamp;
  public Time	configTimestamp;
  public int		ncrtc;
  public RRCrtc	*crtcs;
  public int		noutput;
  public RROutput	*outputs;
  public int		nmode;
  public XRRModeInfo	*modes;
}

public unsafe struct XRRMonitorInfo
{
  public Atom name;
  public int primary; // bool
  public int automatic; // bool
  public int noutput;
  public int x;
  public int y;
  public int width;
  public int height;
  public int mwidth;
  public int mheight;
  public RROutput* outputs;
}

public unsafe struct XRROutputInfo
{
  public Time timestamp;
  public RRCrtc crtc;
  public byte* name;
  public int nameLen;
  public IntPtr mm_width;
  public IntPtr mm_height;
  public Connection connection;
  public SubpixelOrder subpixel_order;
  public int ncrtc;
  public RRCrtc* crtcs;
  public int nclone;
  public RROutput* clones;
  public int nmode;
  public int npreferred;
  public RRMode* modes;
}

public unsafe struct XRRCrtcTransformAttributes 
{
  public XTransform	pendingTransform;
  public byte* pendingFilter;
  public int		pendingNparams;
  public XFixed	*pendingParams;
  public XTransform	currentTransform;
  public byte	*currentFilter;
  public int		currentNparams;
  public XFixed	*currentParams;
}

