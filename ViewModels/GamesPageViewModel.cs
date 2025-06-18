using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UltrawideOverlays.Models;
using UltrawideOverlays.Services;
using System.Drawing;
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
