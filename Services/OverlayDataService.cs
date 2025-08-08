using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UltrawideOverlays.Factories;
using UltrawideOverlays.Models;

namespace UltrawideOverlays.Services
{
    public class OverlayDataService
    {
        private readonly DatabaseProvider _provider;
        private readonly ImageCacheService _cacheService;
        public OverlayDataService(DatabaseProvider db, ImageCacheService cacheService)
        {
            _provider = db;
            _cacheService = cacheService;
        }
        public async Task<ICollection<OverlayDataModel>> LoadAllOverlaysAsync()
        {
            var db = await _provider.GetDatabaseAsync();
            return db.Overlays;
        }

        public async Task<OverlayDataModel?> LoadOverlayAsync(string overlayName)
        {
            var db = await _provider.GetDatabaseAsync();
            var overlay = db.Overlays.FirstOrDefault(o => o.Name.Equals(overlayName, StringComparison.OrdinalIgnoreCase));
            if (overlay != null)
            {
                return overlay;
            }
            else
            {
                return null;
            }
        }

        public async Task SaveOverlayAsync(OverlayDataModel overlay, bool createImage = true)
        {
            var db = await _provider.GetDatabaseAsync();
            var tasks = new List<Task>();
            if (createImage)
            {
                tasks.Add(db.SaveBitmapFromOverlayAsync(overlay));

                _cacheService.InvalidateCache(overlay.Path);
            }

            tasks.Add(db.SaveAsync(overlay, DatabaseFiles.Overlays, true));

            await Task.WhenAll(tasks);
        }

        public async Task DeleteOverlayAsync(OverlayDataModel overlay)
        {
            var db = await _provider.GetDatabaseAsync();

            await db.DeleteAsync(overlay, DatabaseFiles.Overlays);
        }
    }
}
