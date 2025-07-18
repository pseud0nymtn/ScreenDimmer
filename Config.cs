using System.Drawing;
using System.IO;
using System.Text.Json;

namespace ScreenDimmer
{
    // ConfigRoot und MonitorConfig müssen hier bekannt sein:
    public class ConfigRoot
    {
        public List<Config> MonitorConfigs { get; set; }
    }

    public class Config
    {
        public int MonitorIndex { get; set; } = 0;
        public double Brightness { get; set; } = 0.5;
        public string LabelName { get; set; } = "Monitor 1";
        public bool IsEnabled { get; set; } = true;
        public String BackgroundColorHex { get; set; } = "#000000"; // Schwarz

        public static Config Load(string path)
        {
            if (!File.Exists(path))
                return new Config();
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<Config>(json) ?? new Config();
        }
    }
}