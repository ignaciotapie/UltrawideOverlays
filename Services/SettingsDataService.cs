using System;
using System.Diagnostics;
using System.Threading.Tasks;
using UltrawideOverlays.Factories;
using UltrawideOverlays.Models;

namespace UltrawideOverlays.Services
{
    public class SettingsDataService
    {
        public event EventHandler SettingsChanged;

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

            return db.Settings;
        }

        public async Task SaveSettingsAsync(SettingsDataModel settings)
        {
            var db = await _provider.GetDatabaseAsync();

            await db.SaveAsync(settings, DatabaseFiles.Settings, true);

            Debug.WriteLine($"Settings saved: {settings.ToString()}");

            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }

        public SettingsDataModel LoadDefaultSettings()
        {
            return new SettingsDataModel();
        }
    }
}
