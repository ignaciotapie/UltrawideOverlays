using System.Collections.Concurrent;
using UltrawideOverlays.Enums;

namespace UltrawideOverlays.Models
{
    public class SettingsDataModel : ModelBase
    {
        public ConcurrentDictionary<string, SingleSettingModel> SettingsDictionary { get; set; }
        //Default values constructor
        public SettingsDataModel()
        {
            var GridSize = new SingleSettingModel { Name = SettingsNames.GridSize, Value = "50" };
            var GridOpacity = new SingleSettingModel { Name = SettingsNames.GridOpacity, Value = "0.5" };
            var GridColor = new SingleSettingModel { Name = SettingsNames.GridColor, Value = "4294967295" };
            var StartupEnabled = new SingleSettingModel { Name = SettingsNames.StartupEnabled, Value = SettingsBoolValues.False };
            var MinimizeToTray = new SingleSettingModel { Name = SettingsNames.MinimizeToTray, Value = SettingsBoolValues.False };
            var ToggleOverlay = new SingleSettingModel { Name = SettingsNames.ToggleOverlayHotkey, Value = "Ctrl + Shift + O" };
            var OpacityUp = new SingleSettingModel { Name = SettingsNames.OpacityUpHotkey, Value = "Ctrl + Shift + ↑" };
            var OpacityDown = new SingleSettingModel { Name = SettingsNames.OpacityDownHotkey, Value = "Ctrl + Shift + ↓" };
            var QuickOverlay = new SingleSettingModel { Name = SettingsNames.OpenMiniOverlayManager, Value = "Ctrl + Shift + P" };

            SettingsDictionary = new ConcurrentDictionary<string, SingleSettingModel>();
            SettingsDictionary.TryAdd(GridSize.Name, GridSize);
            SettingsDictionary.TryAdd(GridOpacity.Name, GridOpacity);
            SettingsDictionary.TryAdd(GridColor.Name, GridColor);
            SettingsDictionary.TryAdd(StartupEnabled.Name, StartupEnabled);
            SettingsDictionary.TryAdd(MinimizeToTray.Name, MinimizeToTray);
            SettingsDictionary.TryAdd(ToggleOverlay.Name, ToggleOverlay);
            SettingsDictionary.TryAdd(OpacityUp.Name, OpacityUp);
            SettingsDictionary.TryAdd(OpacityDown.Name, OpacityDown);
            SettingsDictionary.TryAdd(QuickOverlay.Name, QuickOverlay);
        }

        public void AddOrUpdate(string key, SingleSettingModel value)
        {
            if (SettingsDictionary.ContainsKey(key))
            {
                SettingsDictionary[key] = value;
            }
            else
            {
                SettingsDictionary.TryAdd(key, value);
            }
        }

        public override object Clone()
        {
            var clone = new SettingsDataModel();
            foreach (var setting in SettingsDictionary)
            {
                clone.AddOrUpdate(setting.Key, setting.Value.Clone() as SingleSettingModel);
            }
            return clone;
        }

        public override string ToString()
        {
            var String = "";

            foreach (var setting in SettingsDictionary)
            {
                String += $"{setting.Key}: {setting.Value.Value}\n";
            }

            return String;
        }
    }
}