using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
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

    private ObservableCollection<MenuItemDTO> _menuItems = new ObservableCollection<MenuItemDTO>();
    public ObservableCollection<MenuItemDTO> MenuItems
    {
        get => _menuItems;
        private set => SetProperty(ref _menuItems, value);
    }

    public ICommand ExitApplication { get; } = new RelayCommand(ExitApplicationExecute);

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
                MenuItems.Add(GetMenuItemFromConfig(mon, win));
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
    }


    private static void ExitApplicationExecute()
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

    private MenuItemDTO GetMenuItemFromConfig(Config config, Window win)
    {
        var parentDto = new MenuItemDTO()
        {
            Header = config.LabelName,
            MonitorIndex = config.MonitorIndex,
            BackgroundColorHex = config.BackgroundColorHex
        };

        var childDto = new MenuItemDTO
        {
            Header = "Helligkeit",
            Brightness = config.Brightness * 100,
            SliderScrollCommand = new RelayCommand<MouseWheelEventArgs>(e =>
            {
                if (e == null || e.Source == null)
                    return;

                var slider = (e.Source as Slider);

                if (slider == null)
                    return;

                // e.Delta ist +120 pro Tick nach oben, –120 nach unten
                slider.Value += slider.SmallChange * Math.Round((double)e.Delta / 120);
                e.Handled = true;
            })
        };

        childDto.BrightnessChanged += (e) =>
        {
            if (e != null)
            {
                win.Dispatcher.Invoke(() => win.Opacity = 1 - (e / 100));
            }
        };

        parentDto.Children.Add(childDto);

        return parentDto;
    }
}
