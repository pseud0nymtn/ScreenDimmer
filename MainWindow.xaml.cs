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

            this.Loaded += (s, e) =>
            {
                MakeWindowClickThrough();
            };
        }

        private void MakeWindowClickThrough()
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            int extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT | WS_EX_LAYERED);
        }

        public void ApplyMonitorSettings(Config mon, List<Rect> screens)
        {
            var screen = screens.ElementAt(mon.MonitorIndex);

            this.WindowStartupLocation = WindowStartupLocation.Manual;
            this.Left = screen.Left;
            this.Top = screen.Top;
            this.Width = screen.Width;
            this.Height = screen.Height;

            this.Opacity = 1 - mon.Brightness;

            try
            {
                var color = (Color)ColorConverter.ConvertFromString(mon.BackgroundColorHex);
                this.Background = new SolidColorBrush(color);
            }
            catch
            {
                this.Background = Brushes.Black;
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