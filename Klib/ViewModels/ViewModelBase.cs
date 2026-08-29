using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Klib.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        protected bool SetProperty<T>(ref T store, T value, [CallerMemberName] string propertyName = "")
        {
            if (Comparer<T>.Default.Compare(value, store) == 0)
            {
                return false;
            }

            store = value;
            this.RaisePropertyChanged(propertyName);

            return true;
        }

        public void RaisePropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
