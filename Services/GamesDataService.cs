using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UltrawideOverlays.Factories;
using UltrawideOverlays.Models;

namespace UltrawideOverlays.Services
{
    public class GamesDataService
    {
        private readonly DatabaseProvider _provider;
        public GamesDataService(DatabaseProvider db)
        {
            _provider = db;
        }
        public async Task<ICollection<GamesModel>> LoadAllGamesAsync()
        {
            var db = await _provider.GetDatabaseAsync();
            return db.Games;
        }

        public async Task<GamesModel?> LoadGameAsync(string gamePath)
        {
            var db = await _provider.GetDatabaseAsync();
            var game = db.Games.FirstOrDefault(o => o.ExecutablePath.Equals(gamePath, StringComparison.OrdinalIgnoreCase));
            if (game != null)
            {
                return game;
            }
            else
            {
                return null;
            }
        }

        public async Task SaveGameAsync(GamesModel game)
        {
            var db = await _provider.GetDatabaseAsync();
            var tasks = new List<Task>();

            tasks.Add(db.SaveAsync(game, DatabaseFiles.Games));

            await Task.WhenAll(tasks);
        }

        public async Task DeleteGameAsync(GamesModel game)
        {
            var db = await _provider.GetDatabaseAsync();

            await db.DeleteAsync(game, DatabaseFiles.Games);
        }
    }
}
