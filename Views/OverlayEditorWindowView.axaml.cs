using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using DynamicData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
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
        AvaloniaProperty.Register<OverlayEditorWindowView, ObservableCollection<ImageWrapper>>(nameof(ItemsSource));

    public ObservableCollection<ImageWrapper> ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }
    
    private DragGridControl? dragGridControl;
    private Dictionary<ImageWrapper?, SelectableImage> modelImageDictionary;
    
    //Disposables
    private CompositeDisposable disposables;
    private IDisposable? _itemsSourceSub;

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

        dragGridControl = this.FindControl<DragGridControl>("MainDragGrid");
        modelImageDictionary = [];

        PositionProperties();
        SetBindings();
    }

    private void SetBindings()
    {
        AddHandler(DragDrop.DropEvent, Drop);
        AddHandler(DragDrop.DragOverEvent, DragOver);

        disposables = new CompositeDisposable();

        disposables.Add(Bind(ItemsSourceProperty, new Binding("Images")));
        disposables.Add(Bind(SelectedProperty, new Binding("Selected")
        {
            Mode = BindingMode.TwoWay
        }));
        
        disposables.Add(ItemsSourceProperty.Changed.Subscribe((args) =>
        {
            HandleItemsSource(args);
        }));
        disposables.Add(SelectedProperty.Changed.Subscribe((args) =>
        {
            HandleImageSelection(args);
        }));
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
 
    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);

        foreach (var kv in modelImageDictionary)
        {
            kv.Key.Dispose();
            kv.Value.Dispose();
        }
        RemoveHandler(DragDrop.DropEvent, Drop);
        RemoveHandler(DragDrop.DragOverEvent, DragOver);

        _itemsSourceSub?.Dispose();
        _itemsSourceSub = null;

        if (ItemsSource != null)
        {
            foreach (var item in ItemsSource)
            {
                item.Dispose();
            }
        }

        ItemsSource?.Clear();
        ItemsSource = null;
        
        Selected = null;
        dragGridControl = null;

        if (disposables != null)
        {
            disposables.Dispose();
        }
        
        (DataContext as IDisposable)?.Dispose();

        modelImageDictionary.Clear();
        modelImageDictionary = null;
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
        if (change.OldValue.Value != null && modelImageDictionary.TryGetValue(change.OldValue.Value as ImageWrapper, out var oldSelectedImage))
        {
            oldSelectedImage.IsSelected = false;
        }
        if (change.NewValue.Value != null && modelImageDictionary.TryGetValue(change.NewValue.Value as ImageWrapper, out var newSelectedImage))
        {
            newSelectedImage.IsSelected = true;
        }
    }

    private void HandleItemsSource(AvaloniaPropertyChangedEventArgs<ObservableCollection<ImageWrapper>> change)
    {
        _itemsSourceSub?.Dispose();
        _itemsSourceSub = null;

        if (change.OldValue.HasValue && change.OldValue.Value is { } oldCol)
        {
            foreach (var im in modelImageDictionary.Keys)
                DisposeImageWrapper(im);
        }

        if (change.NewValue.HasValue && change.NewValue.Value is { } newCol)
        {
            _itemsSourceSub = newCol.WeakSubscribe(ImageCollectionChanged);
            GenerateImages(newCol);
        }
    }

    private void PositionProperties()
    {
        //TODO: Make screen agnostic
        var screen = Screens.Primary;
        if (screen == null) return;
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

    private void DisposeImageWrapper(ImageWrapper? im)
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
        
            dragGridControl?.Children.Add(image);
        }
    }

    private void DragDropResult(IEnumerable<IStorageItem>? storageItems)
    {
        if (storageItems != null)
        {
            var imageFilePaths = new List<String>();
            foreach (var item in storageItems)
            {
                if (FileHandlerUtil.IsValidImagePath(item.TryGetLocalPath()))
                {
                    imageFilePaths.Add(item.TryGetLocalPath());
                }
            }
            if (imageFilePaths.Count > 0)
            {
                AddImagesToViewModel(imageFilePaths);
            }
        }
    }

    private void AddImagesToViewModel(IEnumerable<String> imageFilePaths)
    {
        CommandUtils.ExecuteBoundCommand(this, "AddImageModelCommand", imageFilePaths);
    }

    private void AcceptButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (modelImageDictionary.Count <= 0)
        {
            this.FindControl<ListBox>("Images_ListBox")?.Focus();
            return;
        }

        var pixelSize = new PixelSize((int)Bounds.Width, (int)Bounds.Height);

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
            var imageFilePaths = new List<String>();
            foreach (var file in files)
            {
                if (file.TryGetLocalPath() is { } localPath)
                {
                    imageFilePaths.Add(localPath);
                }
            }
            if (imageFilePaths.Count > 0)
            {
                AddImagesToViewModel(imageFilePaths);
            }
        }
    }
}