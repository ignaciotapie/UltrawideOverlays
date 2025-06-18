using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UltrawideOverlays.Models
{
    public class GamesModel : ModelBase
    {
        public string GameName { get; set; }
        public string OverlayName { get; set; }
        public string ExecutablePath { get; set; }
    }
}
