using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Diagnostics.Contracts;

namespace Neuroflow.Design.MVVM
{
    public interface IRelayCommand : ICommand
    {
        void Refresh();
    }

    [Serializable]
    public abstract class RelayCommandBase<T> : IRelayCommand
    {
        public abstract bool CanExecute(T parameter);

        public abstract void Execute(T parameter);

        public event EventHandler CanExecuteChanged;

        protected virtual void OnCanExecuteChanged(EventArgs e)
        {
            Contract.Requires(e != null);

            var handler = CanExecuteChanged;
            if (handler != null) handler(this, e);
        }

        public void Refresh()
        {
            if (!ViewModel.IsDesignMode)
            {
                OnCanExecuteChanged(EventArgs.Empty);
            }
        }

        bool ICommand.CanExecute(object parameter)
        {
            if (ViewModel.IsDesignMode) return true;
            return CanExecute((T)parameter);
        }

        void ICommand.Execute(object parameter)
        {
            if (!ViewModel.IsDesignMode)
            {
                Execute((T)parameter);
            }
        }
    }
}
