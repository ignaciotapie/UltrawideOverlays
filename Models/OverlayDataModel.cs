using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UltrawideOverlays.Models
{
    public class OverlayDataModel
    {
        public string Name { get; set; }
        public string Path { get; set; }

        public OverlayDataModel()
        {
            Name = string.Empty;
            Path = string.Empty;
        }

        public OverlayDataModel(string overlayName, string overlayPath)
        {
            Name = overlayName;
            Path = overlayPath;
        }
    }
}
