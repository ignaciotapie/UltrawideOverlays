using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using UltrawideOverlays.Models;
using UltrawideOverlays.Services;

namespace UltrawideOverlays.ViewModels
{
    public partial class HomePageViewModel : PageViewModel
    {
        [ObservableProperty]
        private int _amountOfGames;

        [ObservableProperty]
        private int _amountOfOverlays;

        [ObservableProperty]
        private string _lastOverlayUsed;

        [ObservableProperty]
        private ObservableCollection<ActivityLogModel>? _activities;

        private GeneralDataService _generalService;
        private ActivityDataService _activityService;

        public event EventHandler? GoToGamesTab;
        public event EventHandler? GoToOverlaysTab;

        private bool isDisposed = false;

        ///////////////////////////////////////////
        /// CONSTRUCTOR
        ///////////////////////////////////////////

        /// <summary>
        /// Design-only constructor
        /// </summary>
        public HomePageViewModel()
        {
            Page = Enums.ApplicationPageViews.HomePage;
            PageName = "Home";

            AmountOfGames = 10;
            AmountOfOverlays = 5;
            LastOverlayUsed = "Guachines";
            Activities = new ObservableCollection<ActivityLogModel>
            {
                new ActivityLogModel() {Type = ActivityLogType.Games , Timestamp = DateTime.Now.AddMinutes(-10), Action = ActivityLogAction.Added, InvolvedObject = "Pepitos" },
                new ActivityLogModel() {Type = ActivityLogType.Settings , Timestamp = DateTime.Now.AddMinutes(-5), Action = ActivityLogAction.Updated, InvolvedObject = "Gallito" },
                new ActivityLogModel() {Type = ActivityLogType.Overlays  , Timestamp = DateTime.Now.AddMinutes(-2), Action = ActivityLogAction.Removed, InvolvedObject = "Guachines" }
            };
        }

        public HomePageViewModel(GeneralDataService generalService, ActivityDataService activityService)
        {
            Page = Enums.ApplicationPageViews.HomePage;
            PageName = "Home";
            LastOverlayUsed = "None";

            _generalService = generalService;
            _activityService = activityService;

            _activityService.ActivityTriggered += ActivityTriggeredHandler;

            LoadDataAsync();
        }

        ~HomePageViewModel()
        {
            Dispose();
        }

        ///////////////////////////////////////////
        /// PRIVATE FUNCTIONS
        ///////////////////////////////////////////
        private void ActivityTriggeredHandler(ActivityLogModel obj)
        {
            if (obj.Type == ActivityLogType.Overlays && obj.Action == ActivityLogAction.Viewed)
            {
                LastOverlayUsed = _activityService.GetLastOverlayUsed();
            }
        }

        private async Task LoadDataAsync()
        {
            AmountOfGames = await _generalService.GetAmountOfGames();
            AmountOfOverlays = await _generalService.GetAmountOfOverlays();
            Activities = new ObservableCollection<ActivityLogModel>(await _activityService.LoadLastActivities(3));
            LastOverlayUsed = _activityService.GetLastOverlayUsed();
        }

        ///////////////////////////////////////////
        /// COMMANDS
        ///////////////////////////////////////////

        [RelayCommand]
        private void NavigateToGamesPage()
        {
            GoToGamesTab?.Invoke(this, EventArgs.Empty);
        }

        [RelayCommand]
        private void NavigateToOverlaysPage()
        {
            GoToOverlaysTab?.Invoke(this, EventArgs.Empty);
        }

        public override void Dispose()
        {
            if (!isDisposed) 
            {
                isDisposed = true;
                _activityService.ActivityTriggered -= ActivityTriggeredHandler;

                Activities.Clear();
                Activities = null;
                _generalService = null;
                _activityService = null;

                Debug.WriteLine("HomePageViewModel finalized!");
            }
        }
    }
}
