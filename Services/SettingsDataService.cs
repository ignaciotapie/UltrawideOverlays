using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UltrawideOverlays.Factories;
using UltrawideOverlays.Models;
using UltrawideOverlays.Utils;

namespace UltrawideOverlays.Services
{
    public class SettingsDataService
    {
        public event EventHandler<SettingsChangedArgs> SettingsChanged;

        DatabaseProvider _provider;

        public SettingsDataService(DatabaseProvider dbProvider)
        {
            _provider = dbProvider;
        }

        public async Task<string> LoadSettingAsync(string settingName)
        {
            var db = await _provider.GetDatabaseAsync();

            if (db.Settings.SettingsDictionary.TryGetValue(settingName, out var setting))
            {
                return setting.Value;
            }
            else
            {
                throw new ArgumentException($"Setting '{settingName}' not found.");
            }
        }

        public async Task<SettingsDataModel> LoadSettingsAsync()
        {
            var db = await _provider.GetDatabaseAsync();

            return db.Settings.Clone();
        }

        public async Task SaveSettingsAsync(SettingsDataModel newSettings)
        {
            var db = await _provider.GetDatabaseAsync();

            var oldSettings = await LoadSettingsAsync();

            var settingsChanged = CompareSettings(oldSettings, newSettings);

            if (settingsChanged.Count > 0)
            {
                await db.SaveAsync(newSettings, DatabaseFiles.Settings);
                SettingsChanged?.Invoke(this, new SettingsChangedArgs(settingsChanged));
            }
            else
            {
                Debug.WriteLine("No settings changed, not saving.");
            }

        }

        public async Task SaveSettingAsync(SingleSettingModel setting)
        {
            var db = await _provider.GetDatabaseAsync();

            var oldSettings = await LoadSettingsAsync();

            //Grab a clone of oldSettings and update this newer one.
            var newSettings = oldSettings.Clone();
            newSettings.AddOrUpdate(setting.Name, setting);

            await SaveSettingsAsync(newSettings);
        }

        private IList<SingleSettingModel> CompareSettings(SettingsDataModel oldSettings, SettingsDataModel newSettings)
        {
            var changedSettings = new List<SingleSettingModel>();

            foreach (var newSetting in newSettings.SettingsDictionary)
            {
                if (oldSettings.SettingsDictionary.TryGetValue(newSetting.Key, out var oldSetting))
                {
                    if (newSetting.Value.Value != oldSetting.Value)
                    {
                        changedSettings.Add(newSetting.Value);
                    }
                }
            }

            return changedSettings;
        }

        public SettingsDataModel LoadDefaultSettings()
        {
            return new SettingsDataModel();
        }
    }
}
