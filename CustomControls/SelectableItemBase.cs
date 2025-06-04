using Avalonia;
using Avalonia.Controls;
using System;
using System.Reactive.Linq;

namespace UltrawideOverlays.CustomControls
{
    public abstract class SelectableItemBase : Border, IDisposable
    {
        public static readonly StyledProperty<bool> IsSelectedProperty =
            AvaloniaProperty.Register<SelectableItemBase, bool>(nameof(IsSelected));

        public bool IsSelected
        {
            get => GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        public event EventHandler? ItemSelectedChanged;

        protected SelectableItemBase()
        {
            this.GetObservable(IsSelectedProperty).Subscribe(RaiseItemSelected);
        }

        private void RaiseItemSelected(bool isSelected)
        {
            ItemSelectedChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            ItemSelectedChanged = null;
        }
    }
}
