using System;
using UltrawideOverlays.ViewModels;

namespace UltrawideOverlays.Factories
{
    // This is a factory class that creates instances of PageViewModel based on the provided ApplicationPageViews enum.
    public class SubviewFactory
    {
        // Injected factory function to create PageViewModel instances.
        private readonly Func<Enums.Subviews, ViewModelBase> viewFactory;
        public SubviewFactory(Func<Enums.Subviews, ViewModelBase> factory)
        {
            viewFactory = factory;
        }

        public ViewModelBase GetSubViewModel(Enums.Subviews pageName) => viewFactory.Invoke(pageName);
    }
}
