using System.Collections.ObjectModel;
using UltrawideOverlays.Models;

namespace UltrawideOverlays.ViewModels
{
    public partial class OverlayEditorWindowViewModel : ViewModelBase
    {
        ObservableCollection<ImageModel>? Images { get; set; }

        public OverlayEditorWindowViewModel()
        {
            Images = new ObservableCollection<ImageModel>();

            // Example of adding an image to the collection
            Images.Add(new ImageModel("C:\\Users\\Nacho\\source\\repos\\UltrawideOverlays\\Assets\\Images\\pattern.png", "Pattern", "Description1", "Category1", "1920x1080", "PNG"));
        }
    }
}
