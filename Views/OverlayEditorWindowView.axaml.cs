using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using UltrawideOverlays.CustomControls;
using UltrawideOverlays.Models;
using UltrawideOverlays.Utils;
using UltrawideOverlays.ViewModels;

namespace UltrawideOverlays.Views;

public partial class OverlayEditorWindowView : Window
{
    private DragGridControl dragGridControl;
    private Dictionary<ImageModel, SelectableImage> modelImageDictionary;

    private ImageModel? copiedImageModel;
    private OverlayEditorWindowViewModel? vmInstance;

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
    /// OVERRIDE FUNCTIONS
    ///////////////////////////////////////////
    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is OverlayEditorWindowViewModel vm)
        {
            vmInstance = vm;

            vmInstance.PropertyChanging += DataContext_PropertyChanging;
            vmInstance.PropertyChanged += DataContext_PropertyChanged;
        }
    }

    private void DataContext_PropertyChanging(object? sender, PropertyChangingEventArgs e)
    {
        if (e.PropertyName == nameof(OverlayEditorWindowViewModel.Selected))
        {
            if (vmInstance.Selected == null) return;
            modelImageDictionary[vmInstance.Selected].IsSelected = false;
        }
    }

    private void DataContext_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(OverlayEditorWindowViewModel.Selected))
        {
            if (vmInstance.Selected == null) return;
            modelImageDictionary[vmInstance.Selected].IsSelected = true;
        }
    }

    ///////////////////////////////////////////
    /// PRIVATE FUNCTIONS
    ///////////////////////////////////////////
    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        vmInstance = DataContext as OverlayEditorWindowViewModel;

        vmInstance.Images.CollectionChanged += ImageCollectionChanged;
        GenerateImages(vmInstance.Images);
    }

    private void OnUnloaded(object? sender, RoutedEventArgs e)
    {
        vmInstance.Images.CollectionChanged -= ImageCollectionChanged;

        foreach (ImageModel im in vmInstance!.Images)
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
            if (vmInstance != null && vmInstance.DuplicateImageModelCommand.CanExecute(copiedImageModel))
            {
                vmInstance.DuplicateImageModelCommand.Execute(copiedImageModel);
            }
        }
    }
    private void CopySelected()
    {
        var selectedControl = vmInstance.Selected;
        if (selectedControl != null)
        {
            copiedImageModel = selectedControl;
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
        image.ItemSelectedChanged -= OnSelectableImageSelected;

        // Remove from visual tree if needed
        if (image.Parent is Panel parent)
        {
            parent.Children.Remove(image);
        }

        modelImageDictionary.Remove(im);

        image.Dispose();
    }

    private void GenerateImages(IList? images)
    {
        if (images == null) return;
        foreach (ImageModel im in images)
        {
            var image = new SelectableImage(im);
            modelImageDictionary.Add(im, image);

            image.ItemSelectedChanged += OnSelectableImageSelected;

            dragGridControl.Children.Add(image);
        }
    }

    private void OnSelectableImageSelected(object? sender, object e)
    {
        var image = sender as SelectableImage;
        var im = image != null ? image.imageModel : null;

        if (im == null) return;

        vmInstance.SelectImageCommand.Execute(im);
    }

    private void AnalyzeIfValidImage(IEnumerable<IStorageItem>? storageItems)
    {
        if (storageItems != null)
        {
            var imageFilePaths = new List<Uri>();
            foreach (var item in storageItems)
            {
                if (FileHandlerUtil.IsValidImagePath(item.Path))
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
        if (vmInstance != null && vmInstance.AddImageModelCommand.CanExecute(null))
        {
            vmInstance.AddImageModelCommand.Execute(imageFilePaths);
        }
    }

    private void DuplicateButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (vmInstance != null)
        {
            vmInstance.DuplicateImageModelCommand.Execute(null);
        }
    }

    private void DeleteButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (vmInstance != null)
        {
            vmInstance.RemoveImageModelCommand.Execute(null);
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


        var pixelSize = new PixelSize((int)this.Bounds.Width, (int)this.Bounds.Height);
        var overlayName = OverlayNameBox.Text;

        if (vmInstance != null && vmInstance.CreateOverlayCommand.CanExecute(pixelSize))
        {
            vmInstance.CreateOverlayCommand.Execute(pixelSize);
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

    private void AddClippingMaskButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
    }
}