using Avalonia.Controls;
using System.Windows.Input;

namespace UltrawideOverlays.Utils
{
    public static class CommandUtils
    {
        public static void ExecuteBoundCommand(Control control, string commandPath, object parameter = null)
        {
            var dataContext = control.DataContext;
            if (dataContext == null || string.IsNullOrEmpty(commandPath))
                return;

            var property = dataContext.GetType().GetProperty(commandPath);
            var command = property?.GetValue(dataContext) as ICommand;
            if (command?.CanExecute(parameter) == true)
                command.Execute(parameter);
        }
    }
}
