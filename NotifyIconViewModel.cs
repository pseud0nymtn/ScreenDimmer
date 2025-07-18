using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ControlzEx.Standard;
using H.NotifyIcon;
using Newtonsoft.Json;
using Application = System.Windows.Application;

namespace ScreenDimmer;

internal static class NativeMethods
{
    public delegate bool MonitorEnumDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left, Top, Right, Bottom;
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    public static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumDelegate lpfnEnum, IntPtr dwData);
}


public partial class NotifyIconViewModel : ObservableObject
{

    private const string configFileName = "config.json";
    private List<MainWindow> windows = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ShowWindowCommand))]
    public bool canExecuteShowWindow = true;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(HideWindowCommand))]
    public bool canExecuteHideWindow;

    [ObservableProperty]
    private ObservableCollection<MenuItem> menuItems = new ObservableCollection<MenuItem>();

    public NotifyIconViewModel()
    {
        var config = LoadConfig();

        if (config?.MonitorConfigs != null && config.MonitorConfigs.Count > 0)
        {
            var screens = GetAllScreens();
            for (int i = 0; i < config.MonitorConfigs.Count; i++)
            {
                var mon = config.MonitorConfigs[i];
                if (mon.MonitorIndex < 0 || mon.MonitorIndex >= screens.Count)
                    continue;

                var win = new MainWindow();
                menuItems.Add(GetMenuItemFromConfig(mon, win));
                win.ApplyMonitorSettings(mon, screens);
                win.Show();
                windows.Add(win);
            }
        }
        else
        {
            // Fallback: Ein Fenster auf dem Hauptmonitor
            var win = new MainWindow();
            windows.Add(win);
            win.Show();
        }

        // Menüpunkt "Beenden" hinzufügen und an ExitApplication binden
        var exitMenuItem = new MenuItem
        {
            Header = "Beenden",
            Command = ExitApplicationCommand
        };
        menuItems.Add(exitMenuItem);
    }

        


    /// <summary>
    /// Shows a window, if none is already open.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExecuteShowWindow))]
    public void ShowWindow()
    {
        Application.Current.MainWindow ??= new MainWindow();
        Application.Current.MainWindow.Show(disableEfficiencyMode: true);
        CanExecuteShowWindow = false;
        CanExecuteHideWindow = true;
    }

    /// <summary>
    /// Hides the main window. This command is only enabled if a window is open.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExecuteHideWindow))]
    public void HideWindow()
    {
        Application.Current.MainWindow.Hide(enableEfficiencyMode: true);
        CanExecuteShowWindow = true;
        CanExecuteHideWindow = false;
    }


    /// <summary>
    /// Shuts down the application.
    /// </summary>
    [RelayCommand]
    public void ExitApplication()
    {
        Application.Current.Shutdown();
    }

    private ConfigRoot? LoadConfig()
    {
        try
        {
            if (File.Exists(configFileName))
            {
                var json = File.ReadAllText(configFileName);
                return JsonConvert.DeserializeObject<ConfigRoot>(json);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Fehler beim Laden der Konfiguration: " + ex.Message);
        }
        return null;
    }

    /// <summary>
    /// Gibt die Arbeitsbereiche aller angeschlossenen Monitore als Rect-Liste zurück.
    /// </summary>
    private static List<Rect> GetAllScreens()
    {
        // Win32-API: EnumDisplayMonitors
        var screens = new List<Rect>();
        NativeMethods.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero,
            (IntPtr hMonitor, IntPtr hdcMonitor, ref NativeMethods.RECT lprcMonitor, IntPtr dwData) =>
            {
                screens.Add(new Rect(
                    lprcMonitor.Left,
                    lprcMonitor.Top,
                    lprcMonitor.Right - lprcMonitor.Left,
                    lprcMonitor.Bottom - lprcMonitor.Top));
                return true;
            }, IntPtr.Zero);
        return screens;
    }

    private MenuItem GetMenuItemFromConfig(Config config, Window win)
    {
        var menuItem = new MyMenuItem()
        {
            Header = config.LabelName,
            Brightness = config.Brightness * 100,
        };
        menuItem.BrightnessChanged += (e =>
        {
            if (e != null)
            {
                win.Dispatcher.Invoke(() => win.Opacity = 1 - (e / 100));
            }
        });

        return menuItem;       
    }
}
