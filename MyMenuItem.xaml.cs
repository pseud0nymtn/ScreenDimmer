using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace ScreenDimmer
{
    public partial class MyMenuItem : MenuItem, INotifyPropertyChanged
    {
        private double _brightness;

        public event PropertyChangedEventHandler? PropertyChanged;

        public event Action<double>? BrightnessChanged;

        public double Brightness
        {
            get => _brightness;
            set
            {
                if (_brightness != value)
                {
                    _brightness = value;
                    OnPropertyChanged();
                    BrightnessChanged?.Invoke(_brightness);
                }
            }
        }

        public ICommand SliderScrollCommand { get; }

        public MyMenuItem() : base()
        {
            InitializeComponent();
            Brightness = 50;
            SliderScrollCommand = new RelayCommand<MouseWheelEventArgs?>(p => OnSliderScroll(p));
            DataContext = this;
        }

        private void OnSliderScroll(MouseWheelEventArgs? parameter)
        {
            if (parameter == null)
                return;

            if (parameter is MouseWheelEventArgs e)
            {
                if (e.Delta > 0)
                    Brightness = Math.Min(Brightness + 1, 100);
                else
                    Brightness = Math.Max(Brightness - 1, 0);
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
