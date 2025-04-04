using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UltrawideOverlays.ViewModels
{
    public partial class HomePageViewModel : PageViewModel
    {
        [ObservableProperty]
        private string _homePageName = "Home Page";
        public HomePageViewModel()
        {
            PageName = Enums.ApplicationPageViews.HomePage;
        }
    }
}
