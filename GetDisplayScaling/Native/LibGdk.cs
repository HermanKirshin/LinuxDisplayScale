using System;
using System.Runtime.InteropServices;

namespace GetDisplayScaling.Native;

public static unsafe class LibGdk
{
  public const string Lib = "libgdk-3.so.0";
  public static bool Exists => LibDl.CanBeLoaded(Lib);

  [DllImport(Lib)]
  public static extern int gdk_monitor_get_scale_factor(GdkMonitor* monitor);

  [DllImport(Lib)]
  public static extern GdkDisplay* gdk_display_get_default();

  [DllImport(Lib)]
  public static extern int gdk_display_get_n_monitors(GdkDisplay* display);

  [DllImport(Lib)]
  public static extern GdkMonitor* gdk_display_get_monitor(GdkDisplay* display, int monitorNum);

  [DllImport(Lib)]
  public static extern void gdk_monitor_get_geometry(GdkMonitor* monitor, out GdkRectangle geometry);

  [DllImport(Lib)]
  public static extern GdkScreen* gdk_display_get_default_screen(GdkDisplay* display);

  [DllImport(Lib)]
  public static extern IntPtr gdk_x11_screen_get_monitor_output(GdkScreen* screen, int monitorNum);

  [DllImport(Lib)]
  public static extern int gdk_monitor_get_width_mm(GdkMonitor* monitor);

  [DllImport(Lib)]
  public static extern int gdk_monitor_get_height_mm(GdkMonitor* monitor);

  [DllImport(Lib)]
  public static extern bool gdk_monitor_is_primary(GdkMonitor* monitor);

  [DllImport(Lib)]
  [return: MarshalAs(UnmanagedType.LPStr)]
  public static extern string gdk_monitor_get_manufacturer(GdkMonitor* monitor);

  [DllImport(Lib)]
  [return: MarshalAs(UnmanagedType.LPStr)]
  public static extern string gdk_monitor_get_model(GdkMonitor* monitor);





  [DllImport(Lib)]
  public static extern GSettingsSchemaSource* g_settings_schema_source_get_default();

  [DllImport(Lib)]
  public static extern GSettingsSchema* g_settings_schema_source_lookup(GSettingsSchemaSource* source, [MarshalAs(UnmanagedType.LPStr)] string schemaId, bool recursive);

  [DllImport(Lib)]
  public static extern GSettings* g_settings_new([MarshalAs(UnmanagedType.LPStr)] string schemaId);

  [DllImport(Lib)]
  public static extern IntPtr g_settings_get_strv(GSettings* settings, [MarshalAs(UnmanagedType.LPStr)] string key);
}

public struct GSettingsSchemaSource
{
  // We use only pointers for strict typing
}

public struct GSettingsSchema
{
  // We use only pointers for strict typing
}

public struct GSettings
{
  // We use only pointers for strict typing
}

public struct GdkDisplay
{
  // We use only pointers for strict typing
}
    
public struct GdkScreen
{
  // We use only pointers for strict typing
}
    
public struct GdkMonitor
{
  // We use only pointers for strict typing
}

public struct GdkRectangle
{
  public int x;
  public int y;
  public int width;
  public int height;
}