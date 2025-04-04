using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UltrawideOverlays.Views;

namespace UltrawideOverlays.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _firstListItem = "Home";
        [ObservableProperty]
        private string _secondListItem = "Games";
        [ObservableProperty]
        private string _thirdListItem = "Overlays";
        [ObservableProperty]
        private string _settingsTxt = "Settings";

        [ObservableProperty]
        private ViewModelBase _currentPage;

        public MainWindowViewModel()
        {
            _homePageViewModel = new HomePageViewModel();
            _gamesPageViewModel = new GamesPageViewModel();
            _overlaysPageViewModel = new OverlaysPageViewModel();

            CurrentPage = _homePageViewModel;
        }

        private readonly HomePageViewModel _homePageViewModel;
        private readonly GamesPageViewModel _gamesPageViewModel;
        private readonly OverlaysPageViewModel _overlaysPageViewModel;

        [RelayCommand]
        private void NavigateToHomePage()
        {
            CurrentPage = _homePageViewModel;
        }

        [RelayCommand]
        private void NavigateToGamesPage()
        {
            CurrentPage = _gamesPageViewModel;
        }

        [RelayCommand]
        private void NavigateToOverlaysPage()
        {
            CurrentPage = _overlaysPageViewModel;
        }
    }
}
