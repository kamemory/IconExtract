using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Klib;
using Klib.ViewModels;

using IconExtract.Services;
using System.Windows;
using System.IO;
using System.Collections.Specialized;

namespace IconExtract.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            this.ExtractTargets = [];

            _pasteCommand = new(this.OnPaste);
            _allExtractCommand = new(this.OnAllExtract, this.CanAllExtract);
        }

        public void Init()
        {
            string[] arguments = [.. Environment.GetCommandLineArgs().Skip(1)];
            this.SetFilesInternal(arguments);
            if (this.ExtractTargets.Count > 0)
            {
                this.SetCanExecute(false);
                this.AutoExtractStart();
            }
        }

        public void SetFiles(string[] files)
        {
            this.SetFilesInternal(files);
            this.SetCanExecute(this.ExtractTargets.Count > 0);
        }

        public ObservableCollection<FileItem> ExtractTargets { get; private set; }

        public ICommand PasteCommand { get { return _pasteCommand; } }

        public ICommand AllExtractCommand { get { return _allExtractCommand; } }


        private readonly RelayCommand _pasteCommand;
        private readonly RelayCommand _allExtractCommand;
        private bool _canExtract = false;


        private void SetFilesInternal(string[] files)
        {
            this.ExtractTargets.Clear();
            foreach (string file in files)
            {
                ExtractFile? f = ExtractFile.Create(file);
                if (f != null)
                {
                    this.ExtractTargets.Add(new(f));
                }
            }
        }

        private void OnPaste()
        {
            if (Clipboard.ContainsText())
            {
                string clipboardText = Clipboard.GetText();
                string[] pathEntries = [..  clipboardText.Split("\n", StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => p.Trim().Trim('"'))
                    .Where(p => !string.IsNullOrEmpty(p) && File.Exists(p)) ];
                this.SetFiles(pathEntries);
            }
            else if (Clipboard.ContainsFileDropList())
            {
                string[] dropList = [.. Clipboard.GetFileDropList().OfType<string>()];
                string[] pathEntries = [.. dropList.Where(p => !string.IsNullOrEmpty(p) && File.Exists(p))];
                this.SetFiles(pathEntries);
            }
        }

        private void SetCanExecute(bool can)
        {
            _canExtract = can;
            _allExtractCommand.RaiseCanExecuteChanged();
        }

        private bool CanAllExtract()
        {
            return _canExtract;
        }

        private void OnAllExtract()
        {
            this.SetCanExecute(false);
            Task.Run(this.ExtractMain);
        }

        private async void AutoExtractStart()
        {
            await Task.Run(this.ExtractMain);
            App.Current.Shutdown();
        }

        private void ExtractMain()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                foreach (FileItem file in this.ExtractTargets)
                {
                    file.Prepare();
                }
            });

            string outputPath = PathUtil.GetFilePathInExec("out");
            foreach (FileItem file in this.ExtractTargets)
            {
                file.Extract(outputPath);
            }

            App.Current.Dispatcher.Invoke(() => this.SetCanExecute(true));
        }
    }
}
