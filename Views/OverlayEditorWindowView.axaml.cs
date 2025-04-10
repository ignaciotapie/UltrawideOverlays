using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UltrawideOverlays.Converters;
using UltrawideOverlays.Models;
using UltrawideOverlays.Utils;
using UltrawideOverlays.ViewModels;

namespace UltrawideOverlays.Views;

public partial class OverlayEditorWindowView : Window
{
    public OverlayEditorWindowView()
    {
        InitializeComponent();

        AddHandler(DragDrop.DropEvent, Drop);
        AddHandler(DragDrop.DragOverEvent, DragOver);
    }

    private void DragOver(object? sender, DragEventArgs e)
    {
        var bounds = PreviewBorder.Bounds;
        if (bounds.Contains(e.GetPosition(PreviewBorder)));
        {
            e.DragEffects &= DragDropEffects.Copy;
        }

        if (!e.Data.Contains(DataFormats.Files))
            e.DragEffects = DragDropEffects.None;

        e.Handled = true;
    }

    private void Drop(object? sender, DragEventArgs e)
    {
        var bounds = PreviewBorder.Bounds;
        if (bounds.Contains(e.GetPosition(PreviewBorder)))
        {
            var files = e.Data.GetFiles();
            e.DragEffects &= DragDropEffects.Copy;
            AnalyzeIfValidImage(files);
        }

        e.Handled = true;
    }

    private void AnalyzeIfValidImage(IEnumerable<IStorageItem>? storageItems) 
    {
        if (storageItems != null) 
        {
            var imageFilePaths = new List<Uri>();
            foreach (var item in storageItems)
            {
                if (FileHandlerUtil.IsValidImage(item.Path.ToString())) 
                {
                    imageFilePaths.Add(item.Path);
                }
            }
            if (imageFilePaths.Count > 0)
            {
                AddImagesToViewModel(imageFilePaths);
            }
        }
    }

    private void AddImagesToViewModel(IEnumerable<Uri> imageFilePaths)
    {
        var viewModel = (DataContext as OverlayEditorWindowViewModel);
        if (viewModel != null && viewModel.AddImageCommand.CanExecute(null))
        {
            viewModel.AddImageCommand.Execute(imageFilePaths);
        }
    }
}