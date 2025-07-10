using Avalonia;
using System;

namespace UltrawideOverlays.Utils
{
    public static class SettingsUtils
    {
        public static void SetStartup(bool value)
        {
            try
            {
                var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                if (key != null)
                {
                    if (value)
                    {
                        key.SetValue(Application.Current.Name, System.Reflection.Assembly.GetExecutingAssembly().Location);
                    }
                    else
                    {
                        key.DeleteValue(Application.Current.Name, false);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting startup value: {ex.Message}");
            }
        }
    }
}
