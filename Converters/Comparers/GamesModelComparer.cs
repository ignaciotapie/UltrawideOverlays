using System;
using System.Collections.Generic;
using UltrawideOverlays.Models;

namespace UltrawideOverlays.Converters.Comparers
{
    public class GamesModelComparer : IEqualityComparer<GamesModel>
    {
        //Just having the same Executable Path is enough to warrant overwriting.
        public bool Equals(GamesModel x, GamesModel y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;

            return x.ExecutablePath.Equals(y.ExecutablePath, StringComparison.OrdinalIgnoreCase)
                || x.Name.Equals(y.Name, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(GamesModel obj)
        {
            return obj.ExecutablePath.GetHashCode();
        }
    }
}
