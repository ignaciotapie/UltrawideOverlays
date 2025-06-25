using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System.Collections.Generic;

namespace UltrawideOverlays.Views;

public partial class GamesPageView : UserControl
{
    public GamesPageView()
    {
        InitializeComponent();
    }

    //!! THIS SHOULD BE DONE WITH A PROVIDER IN THE VIEWMODEL
    //but I'm lazy and I'm not doing this for just one button.
    private async void BrowseButton_Clicked(object sender, RoutedEventArgs args)
    {
        var topLevel = TopLevel.GetTopLevel(this);

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Executable of Game",
            AllowMultiple = false,
            FileTypeFilter = new List<FilePickerFileType>
            {
                new FilePickerFileType("Executable Files")
                {
                    Patterns = new List<string> { "*.exe"}
                }
            }
        });

        // Check if any files were selected.
        if (files.Count > 0)
        {
            if (DataContext is ViewModels.GamesPageViewModel viewModel)
            {
                viewModel.GetExecutableFromFileCommand.Execute(files);
            }
        }
    }
}