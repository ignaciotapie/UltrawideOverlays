using CommunityToolkit.Mvvm.ComponentModel;

namespace UltrawideOverlays.ViewModels
{
    public partial class SettingsPageViewModel : PageViewModel
    {
        [ObservableProperty]
        private string _settingsPageName = "Settings Page";
        public SettingsPageViewModel()
        {
            PageName = Enums.ApplicationPageViews.SettingsPage;
        }
    }
}
