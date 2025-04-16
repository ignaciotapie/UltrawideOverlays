using System.Collections.Generic;

namespace UltrawideOverlays.Models
{
    public class Database
    {
        public IEnumerable<OverlayDataModel>? Overlays { get; set; }

        public Database()
        {
            Overlays = new List<OverlayDataModel>();
        }
    }
}
