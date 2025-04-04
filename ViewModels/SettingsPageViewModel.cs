using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
