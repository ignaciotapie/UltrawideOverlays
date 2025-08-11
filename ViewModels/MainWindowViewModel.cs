using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Diagnostics;
using UltrawideOverlays.Enums;
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
        private PageViewModel? _currentPage;

        private ApplicationPageViews? pageEnum = null;
        private PageFactory? factory;

        ///////////////////////////////////////////
        /// CONSTRUCTOR
        ///////////////////////////////////////////

        /// <summary>
        /// Design-time only constructor
        /// </summary>
        public MainWindowViewModel()
        {
            CurrentPage = new OverlaysPageViewModel();
        }

        ~MainWindowViewModel()
        {
            Dispose();
        }

        public MainWindowViewModel(PageFactory PFactory)
        {
            factory = PFactory;
            NavigateToHomePage();
        }

        partial void OnCurrentPageChanged(PageViewModel? oldValue, PageViewModel? newValue)
        {
            if (oldValue != null)
            {
                oldValue.Dispose();
            }
        }

        ///////////////////////////////////////////
        /// COMMANDS
        ///////////////////////////////////////////

        [RelayCommand]
        private void NavigateToHomePage()
        {
            if (factory == null || pageEnum == Enums.ApplicationPageViews.HomePage)
            {
                return;
            }
            CurrentPage = factory.GetPageViewModel(Enums.ApplicationPageViews.HomePage);

            var viewModel = CurrentPage as HomePageViewModel;
            if (viewModel != null)
            {
                viewModel.GoToGamesTab -= (s, e) => NavigateToGamesPage(); //Unsuscribe if already suscribed.
                viewModel.GoToOverlaysTab -= (s, e) => NavigateToOverlaysPage();
                viewModel.GoToGamesTab += (s, e) => NavigateToGamesPage();
                viewModel.GoToOverlaysTab += (s, e) => NavigateToOverlaysPage();
            }

            pageEnum = Enums.ApplicationPageViews.HomePage;
        }

        [RelayCommand]
        private void NavigateToGamesPage()
        {
            if (factory == null || pageEnum == Enums.ApplicationPageViews.GamesPage)
            {
                return;
            }
            CurrentPage = factory.GetPageViewModel(Enums.ApplicationPageViews.GamesPage);
            pageEnum = Enums.ApplicationPageViews.GamesPage;
        }

        [RelayCommand]
        private void NavigateToOverlaysPage()
        {
            if (factory == null || pageEnum == Enums.ApplicationPageViews.OverlaysPage)
            {
                return;
            }
            CurrentPage = factory.GetPageViewModel(Enums.ApplicationPageViews.OverlaysPage);
            pageEnum = Enums.ApplicationPageViews.OverlaysPage;
        }

        [RelayCommand]
        private void NavigateToSettingsPage()
        {
            if (factory == null || pageEnum == Enums.ApplicationPageViews.SettingsPage)
            {
                return;
            }

            CurrentPage = factory.GetPageViewModel(Enums.ApplicationPageViews.SettingsPage);
            pageEnum = Enums.ApplicationPageViews.SettingsPage;
        }

        public override void Dispose()
        {
            CurrentPage = null;
            factory = null;

            Debug.WriteLine("MainWindowViewModel finalized!");
            GC.SuppressFinalize(this);
        }
    }
}
