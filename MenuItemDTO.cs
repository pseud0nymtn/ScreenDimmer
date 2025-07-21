using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ScreenDimmer
{
    public class MenuItemDTO : ObservableObject
    {
        public event Action<double> BrightnessChanged;


        public string Header { get; set; }

        private double _brightness;
        public double Brightness 
        { 
            get
            {                 
                return _brightness;
            }
            set
            {   if (_brightness != value)
                {
                    _brightness = value;
                    BrightnessChanged?.Invoke(_brightness);
                    OnPropertyChanged(nameof(Brightness));
                }
            }
        }
        public int MonitorIndex { get; set; }
        public string BackgroundColorHex { get; set; }
        public RelayCommand<MouseWheelEventArgs> SliderScrollCommand { get; set; }
        public ObservableCollection<MenuItemDTO> Children { get; }
            = new ObservableCollection<MenuItemDTO>();
    }
}
