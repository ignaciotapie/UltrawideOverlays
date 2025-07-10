using System.ComponentModel;

namespace UltrawideOverlays.Models
{
    public class SingleSettingModel : ModelBase
    {
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public bool Changed { get; private set; } = false;

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.PropertyName == nameof(Value))
            {
                Changed = true;
            }
        }

        public override object Clone()
        {
            return new SingleSettingModel
            {
                Name = this.Name,
                Value = this.Value,
                Changed = this.Changed
            };
        }
    }
}
