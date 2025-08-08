using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using UltrawideOverlays.CustomControls;
using UltrawideOverlays.Utils;
using UltrawideOverlays.Wrappers;

namespace UltrawideOverlays.Views;

public partial class OverlayEditorWindowView : Window
{
    public static readonly StyledProperty<object?> SelectedProperty =
        AvaloniaProperty.Register<OverlayEditorWindowView, object?>(nameof(Selected));
    public object? Selected
    {
        get => GetValue(SelectedProperty);
        set => SetValue(SelectedProperty, value);
    }

    public static readonly StyledProperty<ObservableCollection<ImageWrapper>> ItemsSourceProperty =
        AvaloniaProperty.Register<OverlayEditorWindowView, ObservableCollection<ImageWrapper>>(nameof(ItemsSourceProperty));

    public ObservableCollection<ImageWrapper> ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    private DragGridControl dragGridControl;
    private Dictionary<object?, SelectableImage> modelImageDictionary;

    ///////////////////////////////////////////
    /// CONSTRUCTOR
    ///////////////////////////////////////////
    public OverlayEditorWindowView()
    {
        InitializeComponent();

        //TODO: Make screen agnostic
        var screen = Screens.Primary;
        if (screen != null)
        {
            Width = screen.Bounds.Width;
            Height = screen.Bounds.Height;
            Position = new PixelPoint(0, 0);
        }

        dragGridControl = MainDragGrid;
        modelImageDictionary = [];

        PositionProperties();
        SetBindings();
    }

    private void SetBindings()
    {
        Bind(ItemsSourceProperty, new Binding("Images"));
        Bind(SelectedProperty, new Binding("Selected")
        {
            Mode = BindingMode.TwoWay
        });
        AddHandler(DragDrop.DropEvent, Drop);
        AddHandler(DragDrop.DragOverEvent, DragOver);

        ItemsSourceProperty.Changed.Subscribe((args) =>
        {
            HandleItemsSource(args);
        });
        SelectedProperty.Changed.Subscribe((args) =>
        {
            HandleImageSelection(args);
        });
        Window.WindowStateProperty.Changed.Subscribe((args) =>
        {
            PositionProperties();
        });
        Window.BoundsProperty.Changed.Subscribe((args) =>
        {
            PositionProperties();
        });

        Unloaded += OnUnloaded;
    }

    ///////////////////////////////////////////
    /// OVERRIDE FUNCTIONS
    ///////////////////////////////////////////
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (e.Source is SelectableItemBase control)
        {
            Selected = control.DataContext;
        }
    }

    ///////////////////////////////////////////
    /// KEY HANDLERS
    ///////////////////////////////////////////
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Delete)
        {
            CommandUtils.ExecuteBoundCommand(this, "RemoveImageModelCommand");
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
        DragDropResult(files);

        e.Handled = true;
    }


    ///////////////////////////////////////////
    /// PRIVATE FUNCTIONS
    ///////////////////////////////////////////

    private void HandleImageSelection(AvaloniaPropertyChangedEventArgs<object> change)
    {
        if (change.OldValue.Value != null && modelImageDictionary.TryGetValue(change.OldValue.Value, out var oldSelectedImage))
        {
            oldSelectedImage.IsSelected = false;
        }
        if (change.NewValue.Value != null && modelImageDictionary.TryGetValue(change.NewValue.Value, out var newSelectedImage))
        {
            newSelectedImage.IsSelected = true;
        }
    }

    private void HandleItemsSource(AvaloniaPropertyChangedEventArgs<ObservableCollection<ImageWrapper>> change)
    {
        if (change.OldValue.Value != null) change.OldValue.Value.CollectionChanged -= ImageCollectionChanged;
        if (change.NewValue.Value != null)
        {
            change.NewValue.Value.CollectionChanged += ImageCollectionChanged;
            GenerateImages(change.NewValue.Value);
        }
    }

    private void PositionProperties()
    {
        //TODO: Make screen agnostic
        var screen = Screens.Primary;
        if (screen == null) return;
        if (!Design.IsDesignMode) DragPanel.SetPosition(PropertiesBorder, new Point(screen.Bounds.Center.X - 400, screen.Bounds.Center.Y - 300));
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
        else if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            foreach (var im in modelImageDictionary.Keys.ToList())
            {
                DisposeImageWrapper(im);
            }
        }
    }

    private void RemoveImages(IList? images)
    {
        if (images != null)
        {
            foreach (ImageWrapper im in images)
            {
                DisposeImageWrapper(im);
            }
        }
    }

    private void DisposeImageWrapper(object? im)
    {
        if (modelImageDictionary.TryGetValue(im, out var image))
        {
            if (image.Parent is Panel parent)
            {
                parent.Children.Remove(image);
            }

            modelImageDictionary.Remove(im);
            image.Dispose();
        }
    }

    private void GenerateImages(IList? images)
    {
        if (images == null) return;
        foreach (ImageWrapper im in images)
        {
            if (modelImageDictionary.ContainsKey(im)) continue; // Skip if the same exact ImageModel already exists
            var image = new SelectableImage(im);
            modelImageDictionary.Add(im, image);

            dragGridControl.Children.Add(image);
        }
    }

    private void DragDropResult(IEnumerable<IStorageItem>? storageItems)
    {
        if (storageItems != null)
        {
            var imageFilePaths = new List<Uri>();
            foreach (var item in storageItems)
            {
                if (FileHandlerUtil.IsValidImagePath(item.TryGetLocalPath()))
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
        CommandUtils.ExecuteBoundCommand(this, "AddImageModelCommand", imageFilePaths);
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

        var pixelSize = new PixelSize((int)Bounds.Width, (int)Bounds.Height);
        var overlayName = OverlayNameBox.Text;

        CommandUtils.ExecuteBoundCommand(this, "CreateOverlayCommand", pixelSize);

        CloseWindow();
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
        CommandUtils.ExecuteBoundCommand(this, "AddClippingMaskCommand", Screens.Primary?.Bounds.Size);
    }

    private void MirrorPositionButtonX_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var pixelSize = new PixelSize((int)Bounds.Width, (int)Bounds.Height);

        CommandUtils.ExecuteBoundCommand(this, "MirrorPositionXCommand", pixelSize);
    }

    private void MirrorPositionButtonY_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var pixelSize = new PixelSize((int)Bounds.Width, (int)Bounds.Height);

        CommandUtils.ExecuteBoundCommand(this, "MirrorPositionYCommand", pixelSize);
    }

    private async void AddButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);

        var patterns = FileHandlerUtil.ValidImageExtensions.Select(ext => $"*{ext}").ToList();
        var patternsString = "Image Files" + string.Join("; ", patterns);

        //Like in GamesView, this should be done with a provider that gives a reference to the window where the filedialog needs to open.
        //I'm lazy and it's not worth it for keeping a clean MVVM, taking into account that the OverlayEditor already needed a lot of non-standard code.
        //https://docs.avaloniaui.net/docs/basics/user-interface/file-dialogs

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Add Images",
            AllowMultiple = true,
            FileTypeFilter = new List<FilePickerFileType>
            {
                new FilePickerFileType(patternsString)
                {
                    Patterns = patterns
                }
            }
        });

        if (files.Count > 0)
        {
            var imageFilePaths = new List<Uri>();
            foreach (var file in files)
            {
                if (file.TryGetLocalPath() is { } localPath)
                {
                    imageFilePaths.Add(new Uri(localPath));
                }
            }
            if (imageFilePaths.Count > 0)
            {
                AddImagesToViewModel(imageFilePaths);
            }
        }
    }

    private void OnUnloaded(object? sender, RoutedEventArgs e)
    {
        foreach (var image in modelImageDictionary)
        {
            image.Value.Dispose();
        }

        RemoveHandler(DragDrop.DropEvent, Drop);
        RemoveHandler(DragDrop.DragOverEvent, DragOver);

        if (ItemsSource != null) ItemsSource.CollectionChanged -= ImageCollectionChanged;
        ItemsSource = null;
        Selected = null;
        dragGridControl = null;

        modelImageDictionary.Clear();

        Unloaded -= OnUnloaded;
    }
}