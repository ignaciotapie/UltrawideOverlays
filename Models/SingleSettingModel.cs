namespace UltrawideOverlays.Models
{
    public class SingleSettingModel : ModelBase
    {
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public override object Clone()
        {
            return new SingleSettingModel
            {
                Name = this.Name,
                Value = this.Value,
            };
        }
    }
}
