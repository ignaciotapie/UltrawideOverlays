using System.Collections.Concurrent;
using UltrawideOverlays.Enums;

namespace UltrawideOverlays.Models
{
    public class SettingsDataModel : ModelBase
    {
        public ConcurrentDictionary<string, SingleSettingModel> Settings { get; set; }
        //Default values constructor
        public SettingsDataModel()
        {
            var GridSize = new SingleSettingModel { Name = SettingsNames.GridSize, Value = "50" };
            var GridOpacity = new SingleSettingModel { Name = SettingsNames.GridOpacity, Value = "0.5" };
            var GridColor = new SingleSettingModel { Name = SettingsNames.GridColor, Value = "#FF0000" };
            var StartupEnabled = new SingleSettingModel { Name = SettingsNames.StartupEnabled, Value = "true" };
            var MinimizeToTray = new SingleSettingModel { Name = SettingsNames.MinimizeToTray, Value = "true" };


            Settings = new ConcurrentDictionary<string, SingleSettingModel>();
            Settings.TryAdd(GridSize.Name, GridSize);
            Settings.TryAdd(GridOpacity.Name, GridOpacity);
            Settings.TryAdd(GridColor.Name, GridColor);
            Settings.TryAdd(StartupEnabled.Name, StartupEnabled);
            Settings.TryAdd(MinimizeToTray.Name, MinimizeToTray);
        }

        public void AddOrUpdate(string key, SingleSettingModel value)
        {
            if (Settings.ContainsKey(key))
            {
                Settings[key] = value;
            }
            else
            {
                Settings.TryAdd(key, value);
            }
        }

        public override object Clone()
        {
            var clone = new SettingsDataModel();
            foreach (var setting in Settings)
            {
                clone.Settings.TryUpdate(setting.Key, setting.Value.Clone() as SingleSettingModel, clone.Settings[setting.Key]);
            }
            return clone;
        }
    }
}