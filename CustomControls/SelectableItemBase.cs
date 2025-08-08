using Avalonia;
using Avalonia.Controls;
using System;
using System.Reactive.Linq;

namespace UltrawideOverlays.CustomControls
{
    public abstract class SelectableItemBase : Border
    {
        public static readonly StyledProperty<bool> IsSelectedProperty =
            AvaloniaProperty.Register<SelectableItemBase, bool>(nameof(IsSelected));

        public bool IsSelected
        {
            get => GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        public SelectableItemBase()
        {
            this.GetObservable(IsSelectedProperty).Subscribe(OnItemSelectedChanged);
        }

        protected abstract void OnItemSelectedChanged(bool isSelected);
    }
}
