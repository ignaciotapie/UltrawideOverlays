namespace UltrawideOverlays.Models
{
    public class SettingsDataModel
    {
        public bool PreviewEnabled { get; set; } = true;
        public int PreviewSize { get; set; } = 50;
        public double PreviewOpacity { get; set; } = 0.5;
        public string PreviewColor { get; set; } = "AliceBlue";
        public bool StartupEnabled { get; set; } = false;
    }
}