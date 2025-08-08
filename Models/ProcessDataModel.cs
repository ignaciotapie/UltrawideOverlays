
using Avalonia.Media.Imaging;

namespace UltrawideOverlays.Models
{
    public class ProcessDataModel : ModelBase
    {
        public string Name { get; set; }

        public Bitmap? Icon { get; set; } //Should never reach DB

        public string Path { get; set; }

        public uint ProcessId { get; set; }

        public override object Clone()
        {
            return new ProcessDataModel
            {
                Name = this.Name,
                Icon = this.Icon,
                Path = this.Path,
                ProcessId = this.ProcessId
            };
        }
    }
}
