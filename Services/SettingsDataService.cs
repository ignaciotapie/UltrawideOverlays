using System.Threading.Tasks;
using UltrawideOverlays.Factories;
using UltrawideOverlays.Models;

namespace UltrawideOverlays.Services
{
    public class SettingsDataService
    {
        DatabaseProvider _provider;

        public SettingsDataService(DatabaseProvider dbProvider)
        {
            _provider = dbProvider;
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
        }

        public SettingsDataModel LoadDefaultSettings()
        {
            return new SettingsDataModel();
        }
    }
}
