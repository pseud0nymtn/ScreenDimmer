using System.Windows;
using H.NotifyIcon;

namespace ScreenDimmer
{
    public partial class App : Application
    {
        private TaskbarIcon? notifyIcon;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            SetupTrayIcon();
        }

        private void SetupTrayIcon()
        {
            try
            {
                notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");
                notifyIcon.ForceCreate();
                if (notifyIcon.ContextMenu != null)
                {
                    notifyIcon.ContextMenu.DataContext = notifyIcon.DataContext;
                }
            }
            catch
            {
                // Fehler beim Initialisieren des TrayIcons ignorieren, App läuft trotzdem weiter
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            notifyIcon?.Dispose();
            base.OnExit(e);
        }
    }
}