using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using UltrawideOverlays.Models;

namespace UltrawideOverlays.ViewModels
{
    public partial class OverlayEditorWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        ObservableCollection<ImageModel> _images;

        public OverlayEditorWindowViewModel()
        {
            Images = 
            [
                //Tried again and again to not use this hardcoded path, but it always throws an error... Ehhh. The real app will use absolute paths, so this is not a problem.
                new ImageModel("C:\\Users\\Nacho\\source\\repos\\UltrawideOverlays\\Assets\\Images\\test.png", "SmileyFace", "Description1", "Category1", "1920x1080", "PNG"),
            ];
        }
    }
}
