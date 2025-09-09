using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace ScreenDimmer
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += (_, _) => MakeWindowClickThrough();
        }

        private void MakeWindowClickThrough()
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            int extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT | WS_EX_LAYERED);
        }

        public void ApplyMonitorSettings(Config mon, List<Rect> screens)
        {
            if (mon.MonitorIndex < 0 || mon.MonitorIndex >= screens.Count)
                return;

            var screen = screens[mon.MonitorIndex];
            WindowStartupLocation = WindowStartupLocation.Manual;
            Left = screen.Left;
            Top = screen.Top;
            Width = screen.Width;
            Height = screen.Height;
            Opacity = 1 - mon.Brightness;

            if (!string.IsNullOrWhiteSpace(mon.BackgroundColorHex) &&
                ColorConverter.ConvertFromString(mon.BackgroundColorHex) is Color color)
            {
                Background = new SolidColorBrush(color);
            }
            else
            {
                Background = Brushes.Black;
            }
        }

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TRANSPARENT = 0x00000020;
        private const int WS_EX_LAYERED = 0x00080000;

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
    }
}