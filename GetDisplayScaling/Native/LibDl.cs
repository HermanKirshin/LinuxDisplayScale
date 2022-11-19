using System.Runtime.InteropServices;

namespace GetDisplayScaling.Native;

public static class LibDl
{
    public const string Lib = "libdl.so.2";

    [DllImport(Lib)]
    public static extern IntPtr dlopen(string path, int flags);

    [DllImport(Lib)]
    public static extern void dlclose(IntPtr handle);

    [DllImport(Lib)]
    public static extern IntPtr dlsym(IntPtr handle, string symbol);

    public const int RTLD_LAZY = 0x00001;
    public const int RTLD_NOW = 0x00002;
    public const int RTLD_BINDING_MASK = 0x3;
    public const int RTLD_NOLOAD = 0x00004;
    public const int RTLD_DEEPBIND = 0x00008;
    public const int RTLD_GLOBAL = 0x00100;
    public const int RTLD_LOCAL = 0;
    public const int RTLD_NODELETE = 0x01000;

    public static bool CanBeLoaded(string path)
    {
        var handle = dlopen(path, LibDl.RTLD_LAZY);
        var result = handle != IntPtr.Zero;
        if(result)
            dlclose(handle);
        return result;
    }
}