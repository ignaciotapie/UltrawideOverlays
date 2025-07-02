using System.Threading.Tasks;
using UltrawideOverlays.Factories;

namespace UltrawideOverlays.Services
{
    public class GeneralDataService
    {
        private readonly DatabaseProvider _provider;

        public GeneralDataService(DatabaseProvider db)
        {
            _provider = db;
        }

        public async Task<int> GetAmountOfGames()
        {
            var db = await _provider.GetDatabaseAsync();
            return db.Games.Count;
        }

        public async Task<int> GetAmountOfOverlays()
        {
            var db = await _provider.GetDatabaseAsync();
            return db.Overlays.Count;
        }
    }
}
