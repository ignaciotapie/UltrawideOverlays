using System;
using System.Collections.Generic;
using UltrawideOverlays.Models;

namespace UltrawideOverlays.Converters.Comparers
{
    public class OverlayDataModelComparer : IEqualityComparer<OverlayDataModel>
    {
        //Just having the same name is enough to warrant overwriting.
        public bool Equals(OverlayDataModel x, OverlayDataModel y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;

            return x.Name.Equals(y.Name, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(OverlayDataModel obj)
        {
            return obj.Name.GetHashCode();
        }
    }
}
