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

        private IDisposable selectableDisposable;

        public bool IsSelected
        {
            get => GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        public SelectableItemBase()
        {
            selectableDisposable = this.GetObservable(IsSelectedProperty).Subscribe(OnItemSelectedChanged);
        }

        protected abstract void OnItemSelectedChanged(bool isSelected);

        public virtual void Dispose()
        {
            if (selectableDisposable != null)
            {
                selectableDisposable.Dispose();
                selectableDisposable = null;
            }
        }
    }
}
