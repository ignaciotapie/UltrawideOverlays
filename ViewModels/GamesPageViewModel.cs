using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
