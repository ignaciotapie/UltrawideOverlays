using Avalonia.Controls;
using System;

namespace UltrawideOverlays.Factories
{
    public class WindowFactory
    {
        private readonly Func<Enums.WindowViews, Window> _windowFactory;

        public WindowFactory(Func<Enums.WindowViews, Window> windowFactory)
        {
            _windowFactory = windowFactory;
        }

        public Window CreateWindow(Enums.WindowViews windowName)
        {
            return _windowFactory(windowName);
        }
    }
}
