using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UltrawideOverlays.Factories;
using UltrawideOverlays.Models;

namespace UltrawideOverlays.Services
{
    public class ActivityDataService
    {
        private readonly DatabaseProvider _provider;
        public ActivityDataService(DatabaseProvider db)
        {
            _provider = db;
        }
        public async Task<IEnumerable<ActivityLogModel>> LoadLastActivities(int amount)
        {
            var db = await _provider.GetDatabaseAsync();
            var activities = db.Activities.Take(amount);
            return activities;
        }

        public async Task SaveActivity(ActivityLogModel activity)
        {
            var db = await _provider.GetDatabaseAsync();

            db.AddActivity(activity);
        }
    }
}
