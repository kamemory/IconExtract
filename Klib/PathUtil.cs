using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Klib
{
    public static class PathUtil
    {
        public static string? GetExecutingDirectory()
        {
            return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        }

        public static string GetFilePathInExec(string fileName)
        {
            string path = GetExecutingDirectory() ?? Environment.CurrentDirectory;
            return Path.Combine(path, fileName);
        }
    }
}
