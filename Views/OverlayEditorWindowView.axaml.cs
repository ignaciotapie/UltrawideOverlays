using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UltrawideOverlays.CustomControls;
using UltrawideOverlays.Models;
using UltrawideOverlays.Utils;
using UltrawideOverlays.ViewModels;

namespace UltrawideOverlays.Views;

public partial class OverlayEditorWindowView : Window
{
    public OverlayEditorWindowViewModel OverlayEditorWindowViewModel
    {
        get => (OverlayEditorWindowViewModel)DataContext;
        set => DataContext = value;
    }

    private Dictionary<ImageModel, Image> modelImageDictionary;
    private DragGridControl dragGridControl;

    public OverlayEditorWindowView()
    {
        InitializeComponent();

        AddHandler(DragDrop.DropEvent, Drop);
        AddHandler(DragDrop.DragOverEvent, DragOver);
        modelImageDictionary = new Dictionary<ImageModel, Image>();
        dragGridControl = this.FindControl<DragGridControl>("MainDragGrid");

        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        OverlayEditorWindowViewModel.Images.CollectionChanged += ImageCollectionChanged;
        GenerateImages(OverlayEditorWindowViewModel.Images);
    }

    private void ImageCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            GenerateImages(e.NewItems);
        }
        else if (e.Action == NotifyCollectionChangedAction.Remove)
        {
            RemoveImages(e.OldItems);
        }
    }
    private void GenerateImages(IList? images)
    {
        foreach (ImageModel im in images)
        {
            var bitmap = new Avalonia.Media.Imaging.Bitmap(im.ImagePath);
            var image = new Image
            {
                Source = bitmap,
                Name = im.ImageName,
                Stretch = Stretch.None,
                Width = bitmap.PixelSize.Width,
                Height = bitmap.PixelSize.Height
            };
            dragGridControl.Children.Add(image);
            modelImageDictionary.Add(im, image);
        }
    }

    private void RemoveImages(IList? images)
    {
        foreach (ImageModel im in images)
        {
            dragGridControl.Children.Remove(modelImageDictionary[im]);
            modelImageDictionary.Remove(im);
        }
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
        var files = e.Data.GetFiles();
        e.DragEffects &= DragDropEffects.Copy;
        AnalyzeIfValidImage(files);

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