using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IconExtract.Services;

using Klib.ViewModels;

namespace IconExtract.ViewModels
{
    public class FileItem : ViewModelBase, IComparer<FileItem>
    {
        public FileItem(ExtractFile file)
        {
            _extractFile = file;
        }

        public void Prepare()
        {
            this.Status = string.Empty;
        }

        public void Extract(string outputDir)
        {
            try
            {
                App.Current.Dispatcher.Invoke(() => { this.Status = "→"; });
                _extractFile.Extract(outputDir);
                App.Current.Dispatcher.Invoke(() => { this.Status = "〇"; });
            }
            catch
            {
                App.Current.Dispatcher.Invoke(() => { this.Status = "×"; });
            }
        }

        public int Compare(FileItem? x, FileItem? y)
        {
            return 1;
        }

        public string Status
        {
            get { return _status; }
            private set { this.SetProperty(ref _status, value); }
        }

        public string FilePath { get { return _extractFile.FilePath; } }


        private readonly ExtractFile _extractFile;
        private string _status = string.Empty;
    }
}
