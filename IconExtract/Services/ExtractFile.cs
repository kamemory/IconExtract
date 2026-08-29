using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

using IWshRuntimeLibrary;

using Klib;

namespace IconExtract.Services
{
    public class ExtractFile
    {
        private ExtractFile(string filePath)
        {
            this.FilePath = filePath;
        }

        public void Extract(string outputBaseDir)
        {
            string baseName = Path.GetFileNameWithoutExtension(this.FilePath);
            string outputDir = Path.Combine(outputBaseDir, baseName);

            List<byte[]> icons = IconExtractor.ExtractIcons(this.FilePath);
            this.SaveIcon(outputDir, icons);
        }

        private void SaveIcon(string outputDir, List<byte[]> icons)
        {
            int nameLength = icons.Count.ToString().Length;

            for (int i = 0; i < icons.Count; i++)
            {
                byte[] icon = icons[i];

                using MemoryStream ms = new(icon);
                IconBitmapDecoder decoder = new(ms, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);

                string iconNo = i.ToString().PadLeft(nameLength, '0');

                foreach (BitmapFrame frame in decoder.Frames)
                {
                    this.SaveIcon(iconNo, outputDir, frame);
                }

                string fileName = $"{iconNo}.ico";
                string iconPath = Path.Combine(outputDir, fileName);
                FileUtil.SaveAllBytes(iconPath, icon);
            }
        }

        public string FilePath { get; private set; }

        private void SaveIcon(string iconNo, string outputDir, BitmapFrame frame)
        {
            string fileName = $"{iconNo}_{frame.Width}x{frame.Height}.png";
            string outputFilePath = Path.Combine(outputDir, fileName);

            using MemoryStream ms = new();
            PngBitmapEncoder encoder = new();
            encoder.Frames.Add(frame);
            encoder.Save(ms);

            FileUtil.SaveAllBytes(outputFilePath, ms.ToArray());
        }

        public static ExtractFile? Create(string filePath)
        {
            if (filePath.EndsWith(".dll", StringComparison.CurrentCultureIgnoreCase) ||
                filePath.EndsWith(".exe", StringComparison.CurrentCultureIgnoreCase))
            {
                return CreateFromNormalFile(filePath);
            }
            else if (filePath.EndsWith(".lnk", StringComparison.CurrentCultureIgnoreCase))
            {
                return CreateFromShortcut(filePath);
            }
            else
            {
                return null;
            }
        }

        private static ExtractFile? CreateFromNormalFile(string filePath)
        {
            if (System.IO.File.Exists(filePath))
            {
                return new(filePath);
            }
            else
            {
                return null;
            }
        }

        private static ExtractFile? CreateFromShortcut(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
            {
                return null;
            }

            string shortcutTarget = string.Empty;
            try
            {
                WshShell shell = new();
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(filePath);
                shortcutTarget = shortcut.TargetPath;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }

            if (shortcutTarget.EndsWith(".dll", StringComparison.CurrentCultureIgnoreCase) ||
                shortcutTarget.EndsWith(".exe", StringComparison.CurrentCultureIgnoreCase))
            {
                return CreateFromNormalFile(shortcutTarget);
            }
            else
            {
                return null;
            }
        }
    }
}
