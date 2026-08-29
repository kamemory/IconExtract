using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Klib
{
    public static class FileUtil
    {
        public static void SaveAllText(string filePath, string text)
        {
            EnsureBaseDirectory(filePath);
            File.WriteAllText(filePath, text);
        }

        public static void SaveAllBytes(string filePath, byte[] bytes)
        {
            EnsureBaseDirectory(filePath);
            File.WriteAllBytes(filePath, bytes);
        }

        private static void EnsureBaseDirectory(string filePath)
        {
            string dir = Path.GetDirectoryName(filePath) ?? string.Empty;
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }
    }
}
