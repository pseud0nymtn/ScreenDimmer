using System;
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
                if (!File.Exists(ConfigFileName))
                    return null;

                // Use FileStream + StreamReader to reduce large string allocations for big files
                using var fs = File.OpenRead(ConfigFileName);
                using var sr = new StreamReader(fs);
                using var jsonReader = new JsonTextReader(sr);
                var serializer = JsonSerializer.CreateDefault();
                var root = serializer.Deserialize<ConfigRoot>(jsonReader);
                return root;
            }
            catch (Exception)
            {
                // Fehlerbehandlung ggf. an den Aufrufer weitergeben
            }
            return null;
        }
    }
}
