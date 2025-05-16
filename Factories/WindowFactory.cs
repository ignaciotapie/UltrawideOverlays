using Avalonia.Controls;
using System;

namespace UltrawideOverlays.Factories
{
    public class WindowFactory
    {
        private readonly Func<Enums.WindowViews, object?, Window> _windowFactory;

        public WindowFactory(Func<Enums.WindowViews, object?, Window> windowFactory)
        {
            _windowFactory = windowFactory;
        }

        public Window CreateWindow(Enums.WindowViews windowName, Object? args = null)
        {
            return _windowFactory(windowName, args);
        }
    }
}
