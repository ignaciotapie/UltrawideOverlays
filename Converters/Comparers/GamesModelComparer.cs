using System;
using System.Collections.Generic;
using UltrawideOverlays.Models;

namespace UltrawideOverlays.Converters.Comparers
{
    public class GamesModelComparer : IEqualityComparer<GamesModel>
    {
        //Just having the same name or path is enough to warrant overwriting.
        public bool Equals(GamesModel x, GamesModel y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;

            return x.ExecutablePath.Equals(y.ExecutablePath, StringComparison.OrdinalIgnoreCase) ||
                   x.GameName.Equals(y.GameName, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(GamesModel obj)
        {
            return obj.ExecutablePath.GetHashCode();
        }
    }
}
