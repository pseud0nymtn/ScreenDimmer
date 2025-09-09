using System.Collections.Generic;

namespace ScreenDimmer
{
    public class ConfigRoot
    {
        public List<Config> MonitorConfigs { get; set; } = new();
    }

    public class Config
    {
        public int MonitorIndex { get; set; } = 0;
        public double Brightness { get; set; } = 0.5;
        public string LabelName { get; set; } = "Monitor 1";
        public bool IsEnabled { get; set; } = true;
        public string BackgroundColorHex { get; set; } = "#000000";
    }
}