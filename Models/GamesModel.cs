namespace UltrawideOverlays.Models
{
    public class GamesModel : ModelBase
    {
        public string Name { get; set; }
        public string OverlayName { get; set; }
        public string ExecutablePath { get; set; }

        public override object Clone()
        {
            return new GamesModel
            {
                Name = this.Name,
                OverlayName = this.OverlayName,
                ExecutablePath = this.ExecutablePath
            };
        }
    }
}
