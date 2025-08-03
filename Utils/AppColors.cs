using Avalonia;
using Avalonia.Media;

namespace UltrawideOverlays.Utils
{
    public static class AppColors
    {
        public static IBrush Get(string key) => Application.Current?.Resources[key] as IBrush ?? Brushes.Transparent;
    }
}
