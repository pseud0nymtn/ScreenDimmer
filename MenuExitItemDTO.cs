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
    public class MenuExitItemDTO : ObservableObject
    {
        public RelayCommand ExitCommand { get; set; }

    }
}
