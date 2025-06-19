using CommunityToolkit.Mvvm.ComponentModel;
using UltrawideOverlays.Factories;

namespace UltrawideOverlays.ViewModels
{
    public partial class GamesPageViewModel : PageViewModel
    {
        [ObservableProperty]
        private PageViewModel _selectedViewModel;

        private PageFactory _factory;

        /// <summary>
        /// Design-only constructor
        /// </summary>
        public GamesPageViewModel()
        {
            Page = Enums.ApplicationPageViews.GamesPage;
            PageName = "Games";
        }

        public GamesPageViewModel(PageFactory PFactory)
        {
            Page = Enums.ApplicationPageViews.GamesPage;
            PageName = "Games";

            _factory = PFactory;
        }
    }
}
