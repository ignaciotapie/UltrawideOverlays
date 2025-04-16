using CommunityToolkit.Mvvm.ComponentModel;

namespace UltrawideOverlays.ViewModels
{
    public partial class GamesPageViewModel : PageViewModel
    {
        [ObservableProperty]
        private string _gamesPageTitle = "Games Page";
        public GamesPageViewModel()
        {
            PageName = Enums.ApplicationPageViews.GamesPage;
        }
    }
}
