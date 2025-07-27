using System;
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

        public event Action<ActivityLogModel> ActivityTriggered;
        private string LastOverlayUsed = "None";

        public ActivityDataService(DatabaseProvider db)
        {
            _provider = db;

            ActivityTriggered += ActivityTriggeredHandler;
        }

        private void ActivityTriggeredHandler(ActivityLogModel obj)
        {
            if (obj.Type == ActivityLogType.Overlays && obj.Action == ActivityLogAction.Viewed)
            {
                LastOverlayUsed = obj.InvolvedObject;
            }
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

            ActivityTriggered?.Invoke(activity);
        }

        public string GetLastOverlayUsed()
        {
            return LastOverlayUsed;
        }
    }
}
