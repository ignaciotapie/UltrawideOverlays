using Avalonia;
using Microsoft.Win32;
using System.IO;

public static class AutoStartUtil
{
    ///////////////////////////////////////////
    /// PUBLIC MEMBERS
    ///////////////////////////////////////////
    public const string SILENT_ARG = "--silent";

    ///////////////////////////////////////////
    /// PRIVATE MEMBERS
    ///////////////////////////////////////////
    private const string RUN_KEY = @"Software\Microsoft\Windows\CurrentVersion\Run";


    ///////////////////////////////////////////
    /// PUBLIC FUNCTIONS
    ///////////////////////////////////////////
    public static void EnableAutostart()
    {
        if (IsAutostartEnabled() || Application.Current == null)
        {
            return;
        }
        string exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName ?? "";

        if (!string.IsNullOrEmpty(exePath) && File.Exists(exePath))
        {
            using (var key = Registry.CurrentUser.OpenSubKey(RUN_KEY, true))
            {
                key?.SetValue(Application.Current?.Name, $"\"{exePath}\" {SILENT_ARG}");
            }
        }
    }

    public static void DisableAutostart()
    {
        using (var key = Registry.CurrentUser.OpenSubKey(RUN_KEY, true))
        {
            key?.DeleteValue(Application.Current?.Name, false);
        }
    }

    public static bool IsAutostartEnabled()
    {
        using (var key = Registry.CurrentUser.OpenSubKey(RUN_KEY, false))
        {
            var value = key?.GetValue(Application.Current?.Name) as string;
            return !string.IsNullOrWhiteSpace(value);
        }
    }
}