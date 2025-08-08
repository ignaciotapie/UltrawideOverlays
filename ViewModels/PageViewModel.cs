using UltrawideOverlays.Enums;

namespace UltrawideOverlays.ViewModels
{
    public abstract partial class PageViewModel : ViewModelBase
    {
        public ApplicationPageViews Page { get; set; }
        public string PageName { get; set; }

        public abstract override void Dispose();
    }
}
