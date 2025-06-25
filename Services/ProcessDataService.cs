using System.Collections.Generic;
using System.Threading.Tasks;
using UltrawideOverlays.Factories;
using UltrawideOverlays.Models;

namespace UltrawideOverlays.Services
{
    public class ProcessDataService
    {
        DatabaseProvider _provider;

        public ProcessDataService(DatabaseProvider dbProvider)
        {
            _provider = dbProvider;
        }

        public async Task<ICollection<ProcessDataModel>> LoadAllProcessesAsync()
        {
            var db = await _provider.GetDatabaseAsync();

            return await Task.Run(() => db.LoadProcesses());
        }
    }
}