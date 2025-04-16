using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UltrawideOverlays.Factories;

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
        private PageViewModel _currentPage;

        private readonly PageFactory factory;

        /// <summary>
        /// Design-time only constructor
        /// </summary>
        public MainWindowViewModel()
        {
            CurrentPage = new SettingsPageViewModel();
        }

        public MainWindowViewModel(PageFactory PFactory)
        {
            factory = PFactory;
            NavigateToHomePage();
        }

        [RelayCommand]
        private void NavigateToHomePage()
        {
            CurrentPage = factory.GetPageViewModel(Enums.ApplicationPageViews.HomePage);
        }

        [RelayCommand]
        private void NavigateToGamesPage()
        {
            CurrentPage = factory.GetPageViewModel(Enums.ApplicationPageViews.GamesPage);
        }

        [RelayCommand]
        private void NavigateToOverlaysPage()
        {
            CurrentPage = factory.GetPageViewModel(Enums.ApplicationPageViews.OverlaysPage);
        }

        [RelayCommand]
        private void NavigateToSettingsPage()
        {
            CurrentPage = factory.GetPageViewModel(Enums.ApplicationPageViews.SettingsPage);
        }
    }
}
