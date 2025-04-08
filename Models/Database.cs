using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
