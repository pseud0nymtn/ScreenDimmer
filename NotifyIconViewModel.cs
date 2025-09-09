using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ControlzEx.Standard;
using H.NotifyIcon;
using Newtonsoft.Json;
using System.Windows.Interop;
using System.Runtime.InteropServices;
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

    // Add SetWindowPos for efficient z-ordering without toggling Topmost
    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
}

public partial class NotifyIconViewModel : ObservableObject
{

    private const string configFileName = "config.json";
    private readonly List<MainWindow> windows = new();

    private ObservableCollection<MenuItemDTO> _menuItems = new ObservableCollection<MenuItemDTO>();
    public ObservableCollection<MenuItemDTO> MenuItems
    {
        get => _menuItems;
        private set => SetProperty(ref _menuItems, value);
    }

    public ICommand ExitApplication { get; } = new RelayCommand(ExitApplicationExecute);

    private readonly DispatcherTimer _topmostTimer;

    // SetWindowPos constants
    private static readonly IntPtr HWND_TOPMOST = new(-1);
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOACTIVATE = 0x0010;

    public NotifyIconViewModel()
    {
        var config = ConfigService.LoadConfig();
        var screens = ScreenService.GetAllScreens();

        var windowsToShow = new List<MainWindow>();

        if (config?.MonitorConfigs != null && config.MonitorConfigs.Count > 0)
        {
            for (int i = 0; i < config.MonitorConfigs.Count; i++)
            {
                var mon = config.MonitorConfigs[i];

                // Skip disabled monitor entries
                if (!mon.IsEnabled)
                    continue;

                if (mon.MonitorIndex < 0 || mon.MonitorIndex >= screens.Count)
                    continue;

                var win = new MainWindow();
                MenuItems.Add(GetMenuItemFromConfig(mon, win));
                win.ApplyMonitorSettings(mon, screens);
                windows.Add(win);
                windowsToShow.Add(win);
            }
        }
        else
        {
            // Fallback: Ein Fenster auf dem Hauptmonitor
            var win = new MainWindow();
            if (screens.Count > 0)
            {
                var fallbackConfig = new Config
                {
                    MonitorIndex = 0,
                    Brightness = 0.0,
                    BackgroundColorHex = "#000000",
                    LabelName = "Hauptmonitor",
                    IsEnabled = true
                };
                win.ApplyMonitorSettings(fallbackConfig, screens);
            }
            windows.Add(win);
            windowsToShow.Add(win);
        }

        // Exit-Button hinzufügen
        MenuItems.Add(new MenuItemDTO
        {
            Header = "Beenden",
            IsExitbutton = true,
            IsSubMenu = false,
            Command = ExitApplication
        });

        // Show windows after configuration to reduce UI thrash
        foreach (var w in windowsToShow)
            w.Show();

        // Use SetWindowPos to keep windows on top without toggling Topmost
        _topmostTimer = new DispatcherTimer(DispatcherPriority.Normal)
        {
            Interval = TimeSpan.FromSeconds(2)
        };
        _topmostTimer.Tick += (s, e) =>
        {
            foreach (var window in windows)
            {
                try
                {
                    var hwnd = new WindowInteropHelper(window).Handle;
                    NativeMethods.SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
                }
                catch
                {
                    // ignore per-window failures
                }
            }
        };
        _topmostTimer.Start();
    }


    private static void ExitApplicationExecute()
    {
        Application.Current.Shutdown();
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
                if (e?.Source is Slider slider)
                {
                    // e.Delta ist +120 pro Tick nach oben, –120 nach unten
                    slider.Value += slider.SmallChange * Math.Round((double)e.Delta / 120);
                    e.Handled = true;
                }
            })
        };

        childDto.BrightnessChanged += (e) =>
        {
            if (e != null)
            {
                // Use BeginInvoke to avoid potential re-entrancy/blocking issues
                win.Dispatcher.BeginInvoke(() => win.Opacity = 1 - (e / 100));
            }
        };

        parentDto.Children.Add(childDto);

        return parentDto;
    }
}
