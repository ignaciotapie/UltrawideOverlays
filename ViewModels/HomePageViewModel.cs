using CommunityToolkit.Mvvm.ComponentModel;

namespace UltrawideOverlays.ViewModels
{
    public partial class HomePageViewModel : PageViewModel
    {
        [ObservableProperty]
        private string _homePageName = "Home Page";
        public HomePageViewModel()
        {
            Page = Enums.ApplicationPageViews.HomePage;
            PageName = "Home";
        }
    }
}
