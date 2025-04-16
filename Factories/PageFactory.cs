using System;
using UltrawideOverlays.ViewModels;

namespace UltrawideOverlays.Factories
{
    // This is a factory class that creates instances of PageViewModel based on the provided ApplicationPageViews enum.
    public class PageFactory
    {
        // Injected factory function to create PageViewModel instances.
        private readonly Func<Enums.ApplicationPageViews, PageViewModel> pageFactory;
        public PageFactory(Func<Enums.ApplicationPageViews, PageViewModel> factory)
        {
            pageFactory = factory;
        }

        public PageViewModel GetPageViewModel(Enums.ApplicationPageViews pageName) => pageFactory.Invoke(pageName);
    }
}
