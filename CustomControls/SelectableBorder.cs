using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System.Collections.Specialized;

namespace UltrawideOverlays.CustomControls
{
    public class SelectableBorder : SelectableItemBase
    {
        public static readonly StyledProperty<IBrush?> SelectedBrushProperty =
            AvaloniaProperty.Register<SelectableBorder, IBrush?>(nameof(SelectedBrush));

        public IBrush? SelectedBrush
        {
            get => GetValue(SelectedBrushProperty);
            set => SetValue(SelectedBrushProperty, value);
        }

        public SelectableBorder()
        {
            Background = Brushes.Transparent;
            BorderThickness = new Thickness(2);
            BorderBrush = null;
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top;
        }

        protected override void LogicalChildrenCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
                    if (item is Control c)
                    {
                        c.IsHitTestVisible = false;
                    }
                }
            }
            base.LogicalChildrenCollectionChanged(sender, e);
        }

        protected override void OnItemSelectedChanged(bool isSelected)
        {
            UpdateBorder();
        }

        private void UpdateBorder()
        {
            BorderBrush = IsSelected ? SelectedBrush : null;
        }
    }
}
