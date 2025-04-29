using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
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
    private DragGridControl dragGridControl;
    private Dictionary<ImageModel, SelectableImage> modelImageDictionary;
    private SelectableImage selected;

    ///////////////////////////////////////////
    /// CONSTRUCTOR
    ///////////////////////////////////////////

    public OverlayEditorWindowView()
    {
        InitializeComponent();

        AddHandler(DragDrop.DropEvent, Drop);
        AddHandler(DragDrop.DragOverEvent, DragOver);
        dragGridControl = MainDragGrid;
        modelImageDictionary = new Dictionary<ImageModel, SelectableImage>();

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnUnloaded(object? sender, RoutedEventArgs e)
    {
        var dataContext = DataContext as OverlayEditorWindowViewModel;
        if (dataContext != null)
        {
            dataContext.Images.CollectionChanged -= ImageCollectionChanged;
        }
    }

    ///////////////////////////////////////////
    /// DRAG-AND-DROP
    ///////////////////////////////////////////

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


    ///////////////////////////////////////////
    /// PRIVATE FUNCTIONS
    ///////////////////////////////////////////
    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        var dataContext = DataContext as OverlayEditorWindowViewModel;

        dataContext.Images.CollectionChanged += ImageCollectionChanged;
        GenerateImages(dataContext.Images);
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

    private void RemoveImages(IList? images)
    {
        if (images != null)
        {
            foreach (ImageModel im in images)
            {
                var image = modelImageDictionary[im];
                image.ImageSelected -= OnSelectableImageSelected;
                dragGridControl.Children.Remove(image);
                modelImageDictionary.Remove(im);
            }
        }
    }

    private void GenerateImages(IList? images)
    {
        foreach (ImageModel im in images)
        {
            var image = new SelectableImage(im);

            modelImageDictionary.Add(im, image);

            image.ImageSelected += OnSelectableImageSelected;

            dragGridControl.Children.Add(image);
        }
    }

    private void OnSelectableImageSelected(object? sender, ImageModel e)
    {
        if (DataContext is OverlayEditorWindowViewModel viewModel && viewModel.SelectImageCommand.CanExecute(e))
        {
            viewModel.SelectImageCommand.Execute(e);
            var image = sender as SelectableImage;
            if (image != null)
            {
                if (selected != null)
                {
                    selected.IsSelected = false;
                }
                selected = image;
                image.IsSelected = true;
            }
            Images_ListBox.SelectedItem = e;
        }
    }
    private void ListBox_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0)
        {
            var selectedItem = e.AddedItems[0] as ImageModel;
            if (selectedItem != null && modelImageDictionary.TryGetValue(selectedItem, out var image))
            {
                image.SelectImage();
            }
        }
    }

    private void AnalyzeIfValidImage(IEnumerable<IStorageItem>? storageItems)
    {
        if (storageItems != null)
        {
            var imageFilePaths = new List<Uri>();
            foreach (var item in storageItems)
            {
                if (FileHandlerUtil.IsValidImagePath(item.Path.ToString()))
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
        if (viewModel != null && viewModel.AddImageModelCommand.CanExecute(null))
        {
            viewModel.AddImageModelCommand.Execute(imageFilePaths);
        }
    }
}