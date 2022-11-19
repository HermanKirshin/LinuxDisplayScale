using System.Runtime.InteropServices;
using GetDisplayScaling.Native;

namespace GetDisplayScaling.SystemInfo;

public static class GSettings
{
    public static unsafe bool IsGnomeWayland => GetSchema() != null;

    // terminal command - check if "gsettings get org.gnome.mutter experimental-features" contains "['scale-monitor-framebuffer']"
    public static bool HasFractionalScaling() => HasOption("scale-monitor-framebuffer");
    
    private static unsafe bool HasOption(string name)
    {
        var schema = GetSchema();
        if (schema == null)
            return false;

        var settings = LibGdk.g_settings_new("org.gnome.mutter");
        if (settings == null)
            return false;
            
        var stringArrayPtr = LibGdk.g_settings_get_strv(settings, "experimental-features");
        while (true)
        {
            var stringPtr = Marshal.ReadIntPtr(stringArrayPtr);
            if (stringPtr == IntPtr.Zero)
                break;
            stringArrayPtr += IntPtr.Size;

            var value = Marshal.PtrToStringAnsi(stringPtr);
            if (value == name)
                return true;
        }

        return false;
    }

    private static unsafe GSettingsSchema* GetSchema()
    {
        if (!LibGdk.Exists)
            return null;
        
        var schemaSource = LibGdk.g_settings_schema_source_get_default();
        return schemaSource == null ? null : LibGdk.g_settings_schema_source_lookup(schemaSource, "org.gnome.mutter", false);
    }
}