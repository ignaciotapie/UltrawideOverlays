using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace UltrawideOverlays.ViewModels
{
    public abstract class ViewModelBase : ObservableObject, IDisposable
    {
        public abstract void Dispose();
    }
}
