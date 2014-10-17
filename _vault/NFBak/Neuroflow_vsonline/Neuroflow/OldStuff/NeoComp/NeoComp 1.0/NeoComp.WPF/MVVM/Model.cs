using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;

namespace NeoComp.WPF.MVVM
{
    [Serializable]
    public abstract class Model : INotifyPropertyChanged
    {
        public Type Type
        {
            get { return GetType(); }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(params string[] propertyNames)
        {
            if (propertyNames == null || propertyNames.Length == 0)
            {
                OnPropertyChanged(new PropertyChangedEventArgs(string.Empty));
            }
            else
            {
                foreach (var pn in propertyNames) OnPropertyChanged(new PropertyChangedEventArgs(pn));
            }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            Contract.Requires(e != null);

            var handler = PropertyChanged;
            if (handler != null) handler(this, e);
        }
    }
}
