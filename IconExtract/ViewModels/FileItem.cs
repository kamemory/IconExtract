using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IconExtract.Services;

namespace IconExtract.ViewModels
{
    public class FileItem : IComparer<FileItem>
    {
        public FileItem(ExtractFile file)
        {
            _extractFile = file;
        }

        public void Extract(string outputDir)
        {
            try
            {
                _extractFile.Extract(outputDir);
            }
            catch
            {
            }
        }

        public int Compare(FileItem? x, FileItem? y)
        {
            return 1;
        }

        public string FilePath { get { return _extractFile.FilePath; } }


        private readonly ExtractFile _extractFile;
    }
}
