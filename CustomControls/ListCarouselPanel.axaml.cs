using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using System;
using System.Collections;

namespace UltrawideOverlays.CustomControls;

public partial class ListCarouselPanel : UserControl
{
    ///////////////////////////////////////////
    /// PROPERTIES
    ///////////////////////////////////////////

    public static readonly StyledProperty<Object?> SelectedProperty =
        AvaloniaProperty.Register<ListCarouselPanel, Object?>(nameof(Selected));

    public Object? Selected
    {
        get => GetValue(SelectedProperty);
        set => SetValue(SelectedProperty, value);
    }

    public static readonly StyledProperty<Boolean> SelectionEnabledProperty =
        AvaloniaProperty.Register<ListCarouselPanel, Boolean>(nameof(SelectionEnabled), true);

    public Boolean SelectionEnabled
    {
        get => GetValue(SelectionEnabledProperty);
        set => SetValue(SelectionEnabledProperty, value);
    }

    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty =
        AvaloniaProperty.Register<ListCarouselPanel, IDataTemplate?>(nameof(ItemTemplate));

    public IDataTemplate? ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        AvaloniaProperty.Register<ListCarouselPanel, IEnumerable?>(nameof(ItemsSource));

    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    ///////////////////////////////////////////
    /// PRIVATE FIELDS
    ///////////////////////////////////////////
    private SelectableItemBase? _lastSelectedItem;

    ///////////////////////////////////////////
    /// CONSTRUCTOR
    ///////////////////////////////////////////
    public ListCarouselPanel()
    {
        InitializeComponent();
    }

    ///////////////////////////////////////////
    /// PUBLIC FUNCTIONS
    ///////////////////////////////////////////


    ///////////////////////////////////////////
    /// OVERRIDE FUNCTIONS
    ///////////////////////////////////////////

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (SelectionEnabled)
        {
            if (e.Source is SelectableItemBase selectableItem)
            {
                SelectItem(selectableItem);
            }
            else if (Selected != null)
            {
                Selected = null;
                CleanLastSelected();
            }
        }
    }

    ///////////////////////////////////////////
    /// PRIVATE FUNCTIONS
    ///////////////////////////////////////////
    private void SelectItem(SelectableItemBase item)
    {
        if (item == null || !SelectionEnabled)
        {
            return;
        }
        CleanLastSelected();
        item.IsSelected = !item.IsSelected;
        Selected = item.IsSelected ? item.DataContext : null;
        _lastSelectedItem = item.IsSelected ? item : null;
    }

    private void CleanLastSelected()
    {
        if (_lastSelectedItem != null)
        {
            _lastSelectedItem.IsSelected = false;
            _lastSelectedItem = null;
        }
    }
}