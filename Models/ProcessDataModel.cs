
using Avalonia.Media.Imaging;

namespace UltrawideOverlays.Models
{
    public class ProcessDataModel : ModelBase
    {
        public string Name { get; set; }

        public Bitmap? Icon { get; set; }

        public string Path { get; set; }

        public uint ProcessId { get; set; }
    }
}
