using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace ScreenDimmer
{
    public static class ConfigService
    {
        private const string ConfigFileName = "config.json";

        public static ConfigRoot? LoadConfig()
        {
            try
            {
                if (File.Exists(ConfigFileName))
                {
                    var json = File.ReadAllText(ConfigFileName);
                    return JsonConvert.DeserializeObject<ConfigRoot>(json);
                }
            }
            catch (Exception ex)
            {
                // Fehlerbehandlung ggf. an den Aufrufer weitergeben
            }
            return null;
        }
    }
}
