using Avalonia.Controls;
using System.Windows;

namespace UltrawideOverlays.Views
{
    public partial class MainWindow : Window
    {
        public OverlayView? window;

        public MainWindow()
        {
            InitializeComponent();
        }

        //TODO: Change this to VM
        private void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (window != null) 
            {
                window.Close();
                window = null;
            }

            window = new OverlayView();

            Win32Properties.AddWindowStylesCallback(
            window,
            (style, exStyle) => (style, exStyle | 0x00000020 | 0x80000));

            window.Show();
        }
    }
}