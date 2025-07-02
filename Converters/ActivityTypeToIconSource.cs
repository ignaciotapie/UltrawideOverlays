using Avalonia;
using Avalonia.Data.Converters;
using System;
using System.Globalization;
using UltrawideOverlays.Models;

namespace UltrawideOverlays.Converters
{
    public class ActivityTypeToIconSource : IValueConverter
    {
        public static readonly ActivityTypeToIconSource Instance = new();

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not ActivityLogType activityType)
                return AvaloniaProperty.UnsetValue;

            try
            {
                return GetSource(activityType);
            }
            catch
            {
                // Fail silently, return unset
            }

            return AvaloniaProperty.UnsetValue;
        }

        private object GetSource(ActivityLogType activityType)
        {
            string iconPath = activityType switch
            {
                ActivityLogType.Games => "avares://UltrawideOverlays/Assets/Images/joystick-logo-colored.svg",
                ActivityLogType.Overlays => "avares://UltrawideOverlays/Assets/Images/overlay-logo-colored.svg",
                ActivityLogType.Settings => "avares://UltrawideOverlays/Assets/Images/settings-icon-colored.svg",
                _ => throw new ArgumentOutOfRangeException(nameof(activityType), activityType, null)
            };

            return iconPath;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
