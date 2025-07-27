using Avalonia.Media;
using System;
using UltrawideOverlays.Utils;

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

        private ActivityLogType _type;
        public ActivityLogType Type { get => _type; set => SetType(value); }
        public string InvolvedObject { get; set; }
        public IBrush TypeColor { get; private set; }

        private void SetType(ActivityLogType value)
        {
            _type = value;

            TypeColor = value switch
            {
                ActivityLogType.Games => AppColors.Get("GameColor"),
                ActivityLogType.Overlays => AppColors.Get("OverlayColor"),
                ActivityLogType.Settings => AppColors.Get("SettingsColor"),
            };
        }

        public string TypeString => Type switch
        {
            ActivityLogType.Games => "Game",
            ActivityLogType.Overlays => "Overlay",
            ActivityLogType.Settings => "Settings",
            _ => "Unknown"
        };
        public string ActionString => Action switch
        {
            ActivityLogAction.Added => "added",
            ActivityLogAction.Removed => "removed",
            ActivityLogAction.Updated => "updated",
            ActivityLogAction.Viewed => "viewed",
            ActivityLogAction.Executed => "triggered",
            _ => "unknown action"
        };
        public string Message => ToString();

        public ActivityLogModel()
        {
            Timestamp = DateTime.Now;
            Action = ActivityLogAction.Viewed;
            Type = ActivityLogType.Games;
            InvolvedObject = "Unknown";
        }
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

            outString += TypeString + " ";

            outString += InvolvedObject + " ";

            outString += ActionString;

            return outString;
        }
    }
}
