using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System.Collections.Generic;
using System.Linq;
using UltrawideOverlays.Utils;

namespace UltrawideOverlays.Views;

public partial class GamesPageView : UserControl
{
    public GamesPageView()
    {
        InitializeComponent();

        DragDropRectangle.AddHandler(DragDrop.DragOverEvent, DragOver);
        DragDropRectangle.AddHandler(DragDrop.DropEvent, Drop);
    }

    private void DragOver(object? sender, DragEventArgs e)
    {
        e.DragEffects &= DragDropEffects.Copy;

        if (!e.Data.Contains(DataFormats.Files))
            e.DragEffects = DragDropEffects.None;

        e.Handled = true;
    }
    private void Drop(object? sender, DragEventArgs e)
    {
        var files = e.Data.GetFiles()?.OfType<IStorageFile>().ToList();

        e.DragEffects &= DragDropEffects.Copy;
        e.Handled = true;

        if (files != null && DataContext is ViewModels.GamesPageViewModel viewModel)
        {
            viewModel.GetExecutableFromFileCommand.Execute(files);
        }
    }

    //!! THIS SHOULD BE DONE WITH A PROVIDER IN THE VIEWMODEL
    //but I'm lazy and I'm not doing this for just one button.
    private async void BrowseButton_Clicked(object sender, RoutedEventArgs args)
    {
        var topLevel = TopLevel.GetTopLevel(this);

        var patterns = FileHandlerUtil.ValidExecutableExtensions.Select(p => $"*{p}").ToList();
        var patternsString = "Executable Files " + string.Join("; ", patterns);

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Executable of Game",
            AllowMultiple = false,
            FileTypeFilter = new List<FilePickerFileType>
            {
                new FilePickerFileType(patternsString)
                {
                    Patterns = patterns
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