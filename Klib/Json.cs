using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace Klib
{
    public static class Json
    {
        public static void SaveToFile(object obj, string path)
        {
            string dir = Path.GetDirectoryName(path) ?? string.Empty;
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            JsonSerializerSettings s = new()
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
            };
            string jsonText = JsonConvert.SerializeObject(obj, s);
            File.WriteAllText(path, jsonText);
        }

        public static T? LoadFromFile<T>(string path)
            where T : class
        {
            if (!File.Exists(path))
            {
                return null;
            }

            string jsonText = File.ReadAllText(path);
            JsonSerializerSettings s = new()
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
            };
            return JsonConvert.DeserializeObject<T>(jsonText);
        }
    }
}
