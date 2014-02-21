using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows.Threading;
using System.Windows;
using System.ComponentModel;
using NeoComp.Internal;

namespace NeoComp.WPF.MVVM
{
    [Serializable]
    public abstract class ViewModel : Model
    {
        private static Dictionary<Type, PropertyInfo[]> commandPropReg = new Dictionary<Type, PropertyInfo[]>();

        #region Construct and Init

        static ViewModel()
        {
            DependencyProperty prop = DesignerProperties.IsInDesignModeProperty;
            IsDesignMode = (bool)DependencyPropertyDescriptor.FromProperty(prop, typeof(FrameworkElement)).Metadata.DefaultValue;
        }

        public ViewModel()
        {
            Initialize();
            if (IsDesignMode)
            {
                DesignModeInitialize();
            }
            else
            {
                RTMModeInitialize();
            }
        }

        protected virtual void Initialize()
        {
        }

        protected virtual void DesignModeInitialize()
        {
            Commands = new ReadOnlyCollection<ICommand>(new ICommand[0]);
        }

        protected virtual void RTMModeInitialize()
        {
            CreateCommands();
            RegisterCommands();
        }

        #endregion

        public static bool IsDesignMode { get; private set; }

        public ReadOnlyCollection<ICommand> Commands { get; private set; }

        public Dispatcher Dispatcher
        {
            get { return Application.Current.Dispatcher; }
        }

        protected virtual void CreateCommands() { }

        private void RegisterCommands()
        {
            var list = new LinkedList<ICommand>();
            var pis = commandPropReg.GetOrRegister(GetType(), 
                () => GetType().GetProperties().Where(pi => pi.PropertyType.GetInterfaces().Any(ti => ti == typeof(ICommand))).ToArray());
            foreach (var pi in pis)
            {
                var cmd = (ICommand)pi.GetValue(this, null);
                if (cmd != null) list.AddLast(cmd);
            }
            Commands = Array.AsReadOnly(list.ToArray());
        }

        public void RefreshAllCommands()
        {
            if (!IsDesignMode)
            {
                foreach (var cmd in Commands)
                {
                    var rc = cmd as IRelayCommand;
                    if (rc != null) rc.Refresh();
                }
            }
        }
    }
}
