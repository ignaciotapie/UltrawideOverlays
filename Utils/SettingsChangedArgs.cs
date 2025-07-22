using System;
using System.Collections.Generic;
using UltrawideOverlays.Models;

namespace UltrawideOverlays.Utils
{
    public class SettingsChangedArgs : EventArgs
    {
        public ICollection<SingleSettingModel> SettingsChanged { get; set; }

        public SettingsChangedArgs(ICollection<SingleSettingModel> settingsChanged)
        {
            SettingsChanged = settingsChanged ?? throw new ArgumentNullException(nameof(settingsChanged), "Settings changed collection cannot be null.");
        }
    }
}
