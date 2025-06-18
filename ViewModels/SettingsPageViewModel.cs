using CommunityToolkit.Mvvm.ComponentModel;

namespace UltrawideOverlays.ViewModels
{
    public partial class SettingsPageViewModel : PageViewModel
    {
        [ObservableProperty]
        private string _settingsPageName = "Settings Page";

        public SettingsPageViewModel()
        {
            Page = Enums.ApplicationPageViews.SettingsPage;
            PageName = "Settings";
        }
    }
}
