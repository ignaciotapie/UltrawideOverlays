using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UltrawideOverlays.Factories;
using UltrawideOverlays.Models;

namespace UltrawideOverlays.Services
{
    public class OverlayDataService
    {
        private readonly DatabaseProvider _provider;
        public OverlayDataService(DatabaseProvider db)
        {
            _provider = db;
        }
        public async Task<List<OverlayDataModel>> LoadAllOverlaysAsync()
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

        public async Task SaveOverlayAsync(OverlayDataModel overlay)
        {
            var db = await _provider.GetDatabaseAsync();
            await db.SaveAsync(overlay, DatabaseFiles.Overlays);
        }
    }
}
