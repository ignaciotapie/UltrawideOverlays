using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using UltrawideOverlays.CustomControls;
using UltrawideOverlays.Models;
using UltrawideOverlays.Utils;
using UltrawideOverlays.ViewModels;

namespace UltrawideOverlays.Views;

public partial class OverlayEditorWindowView : Window
{
    private DragGridControl dragGridControl;
    private Dictionary<ImageModel, SelectableImage> modelImageDictionary;
    private SelectableImage? selected;
    private ImageModel? copiedImageModel;

    ///////////////////////////////////////////
    /// CONSTRUCTOR
    ///////////////////////////////////////////
    public OverlayEditorWindowView()
    {
        InitializeComponent();

        PositionProperties();

        //TODO: Make screen agnostic
        var screen = Screens.Primary;
        if (screen != null)
        {
            this.Width = screen.Bounds.Width;
            this.Height = screen.Bounds.Height;
            this.Position = new PixelPoint(0, 0);
        }
        dragGridControl = MainDragGrid;
        modelImageDictionary = new Dictionary<ImageModel, SelectableImage>();

        AddHandler(DragDrop.DropEvent, Drop);
        AddHandler(DragDrop.DragOverEvent, DragOver);
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        PropertyChanged += OnPropertyChanged;


        AddHandler(KeyDownEvent, DeleteHandler);

    }

    ~OverlayEditorWindowView()
    {
        Debug.WriteLine("OverlayEditorWindowView destructor called");
    }

    private void PositionProperties()
    {
        //TODO: Make screen agnostic
        var screen = Screens.Primary;
        if (screen == null) return;
        if (!Design.IsDesignMode) DragPanel.SetPosition(PropertiesBorder, new Point(screen.Bounds.Center.X - 400, screen.Bounds.Center.Y - 300));
    }

    private void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == Window.WindowStateProperty)
        {
            PositionProperties();
        }
        else if (e.Property == Window.BoundsProperty)
        {
            PositionProperties();
        }
    }

    ///////////////////////////////////////////
    /// KEY HANDLERS
    ///////////////////////////////////////////
    private void DeleteHandler(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Delete)
        {
            DeleteButton_Click(sender, e);
        }
        else if (e.Key == Key.C && e.KeyModifiers == KeyModifiers.Control)
        {
            CopySelected();
        }
        else if (e.Key == Key.V && e.KeyModifiers == KeyModifiers.Control)
        {
            PasteSelected();
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

        dataContext!.Images.CollectionChanged += ImageCollectionChanged;
        GenerateImages(dataContext.Images);
    }

    private void OnUnloaded(object? sender, RoutedEventArgs e)
    {
        var dataContext = DataContext as OverlayEditorWindowViewModel;
        if (dataContext != null)
        {
            dataContext.Images.CollectionChanged -= ImageCollectionChanged;
        }

        foreach (ImageModel im in dataContext!.Images)
        {
            DisposeImageModel(im);
        }

        Loaded -= OnLoaded;
        Unloaded -= OnUnloaded;
    }

    private void PasteSelected()
    {
        if (copiedImageModel != null)
        {
            var viewModel = (DataContext as OverlayEditorWindowViewModel);
            if (viewModel != null && viewModel.DuplicateImageModelCommand.CanExecute(copiedImageModel))
            {
                viewModel.DuplicateImageModelCommand.Execute(copiedImageModel);
            }
        }
    }
    private void CopySelected()
    {
        if (selected != null)
        {
            copiedImageModel = selected.imageModel;
        }
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
                DisposeImageModel(im);
            }
        }
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }

    private void DisposeImageModel(ImageModel im)
    {
        var image = modelImageDictionary[im];
        image.ImageSelected -= OnSelectableImageSelected;

        // Remove from visual tree if needed
        if (image.Parent is Panel parent)
        {
            parent.Children.Remove(image);
        }

        modelImageDictionary.Remove(im);

        if (selected == image)
        {
            selected.IsSelected = false;
            selected = null;
        }

        image.Dispose();
    }

    private void GenerateImages(IList? images)
    {
        if (images == null) return;
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

    private void DuplicateButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var viewModel = (DataContext as OverlayEditorWindowViewModel);
        if (viewModel != null && viewModel.DuplicateImageModelCommand.CanExecute(selected?.imageModel))
        {
            viewModel.DuplicateImageModelCommand.Execute(selected?.imageModel);
        }
    }

    private void DeleteButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var viewModel = (DataContext as OverlayEditorWindowViewModel);
        if (viewModel != null && viewModel.RemoveImageModelCommand.CanExecute(selected?.imageModel))
        {
            viewModel.RemoveImageModelCommand.Execute(selected?.imageModel);
        }
    }

    private void AcceptButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(OverlayNameBox.Text))
        {
            OverlayNameBox.Focus();
            return;
        }
        if (modelImageDictionary.Count <= 0)
        {
            Images_ListBox.Focus();
            return;
        }


        var viewModel = (DataContext as OverlayEditorWindowViewModel);
        var pixelSize = new PixelSize((int)this.Bounds.Width, (int)this.Bounds.Height);
        var overlayName = OverlayNameBox.Text;

        if (viewModel != null && viewModel.CreateOverlayCommand.CanExecute(pixelSize))
        {
            viewModel.CreateOverlayCommand.Execute(pixelSize);
            CloseWindow();
        }
    }

    private void CancelButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        CloseWindow();
    }

    private void CloseWindow()
    {
        this.Close();
    }
}