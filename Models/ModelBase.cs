using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace UltrawideOverlays.Models
{
    public class ModelBase : ObservableObject, ICloneable
    {
        public virtual object Clone()
        {
            return MemberwiseClone();
        }
    }
}
