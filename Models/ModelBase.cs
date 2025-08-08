using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace UltrawideOverlays.Models
{
    public abstract class ModelBase : ObservableObject, ICloneable
    {
        public abstract object Clone();
    }
}
