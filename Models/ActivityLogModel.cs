using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UltrawideOverlays.Models
{
    public enum ActivityLogType 
    {
        Games,
        Overlays,
        Settings
    }

    public enum ActivityLogAction 
    {
        Added,
        Removed,
        Updated,
        Viewed,
        Executed
    }

    public class ActivityLogModel
    {
        public DateTime Timestamp { get; set; }
        public ActivityLogAction Action { get; set; }
        public ActivityLogType Type { get; set; }
        public string InvolvedObject { get; set; }

        public ActivityLogModel(DateTime timestamp, ActivityLogType type, ActivityLogAction action, string involvedObject)
        {
            Timestamp = timestamp;
            Action = action;
            Type = type;
            InvolvedObject = involvedObject;
        }

        public override string ToString()
        {
            var outString = "";

            switch (Type)
            {
                case ActivityLogType.Games:
                    outString += "Game ";
                    break;
                case ActivityLogType.Overlays:
                    outString += "Overlay ";
                    break;
                case ActivityLogType.Settings:
                    outString += "Settings ";
                    break;
            }

            outString += InvolvedObject + " ";

            switch (Action)
            {
                case ActivityLogAction.Added:
                    outString += "added";
                    break;
                case ActivityLogAction.Removed:
                    outString += "removed";
                    break;
                case ActivityLogAction.Updated:
                    outString += "was updated";
                    break;
                case ActivityLogAction.Viewed:
                    outString += "was viewed";
                    break;
                case ActivityLogAction.Executed:
                    outString += "triggered";
                    break;
            }

            return outString;
        }
    }
}
