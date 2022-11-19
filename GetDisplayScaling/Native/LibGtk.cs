using System;
using System.Runtime.InteropServices;

namespace GetDisplayScaling.Native;

public static class LibGtk
{
  public const string Lib = "libgtk-3.so.0";
  public static bool Exists => LibDl.CanBeLoaded(Lib);
  
  [DllImport(Lib)]
  public static extern bool gtk_init_check(int argc, IntPtr argv);
}