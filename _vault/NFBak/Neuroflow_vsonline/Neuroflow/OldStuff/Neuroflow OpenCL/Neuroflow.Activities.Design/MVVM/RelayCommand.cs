using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Neuroflow.Activities.Design.MVVM
{
    [Serializable]
    public class RelayCommand<T> : RelayCommandBase<T>
    {
        public RelayCommand(Action<T> execute)
        {
            Contract.Requires(execute != null);

            this.execute = execute;
        }

        public RelayCommand(Func<T, bool> canExecute, Action<T> execute)
        {
            Contract.Requires(execute != null);
            Contract.Requires(canExecute != null);

            this.execute = execute;
            this.canExecute = canExecute;
        }

        Action<T> execute;

        Func<T, bool> canExecute;

        public sealed override bool CanExecute(T parameter)
        {
            return canExecute != null ? canExecute(parameter) : true;
        }

        public sealed override void Execute(T parameter)
        {
            using (new HourGlass())
            {
                execute(parameter);
            }
        }
    }

    public class RelayCommand : RelayCommand<object>
    {
        public RelayCommand(Action execute)
            : base((o) => execute())
        {
            Contract.Requires(execute != null);
        }

        public RelayCommand(Func<bool> canExecute, Action execute)
            : base((o) => canExecute(), (o) => execute())
        {
            Contract.Requires(execute != null);
            Contract.Requires(canExecute != null);
        }
    }
}
