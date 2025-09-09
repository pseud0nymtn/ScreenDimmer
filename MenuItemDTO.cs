using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ScreenDimmer
{
    public class MenuItemDTO : ObservableObject
    {
        public event Action<double>? BrightnessChanged;

        public bool IsExitbutton { get; set; } = false;
        public bool IsSubMenu { get; set; } = true;
        public string Header { get; set; } = string.Empty;
        public int MonitorIndex { get; set; }
        public string BackgroundColorHex { get; set; } = "#000000";
        public RelayCommand<MouseWheelEventArgs>? SliderScrollCommand { get; set; }
        public ObservableCollection<MenuItemDTO> Children { get; } = new();
        public ICommand? Command { get; set; }

        private double _brightness;
        public double Brightness
        {
            get => _brightness;
            set
            {
                if (SetProperty(ref _brightness, value))
                {
                    BrightnessChanged?.Invoke(_brightness);
                }
            }
        }
    }
}
