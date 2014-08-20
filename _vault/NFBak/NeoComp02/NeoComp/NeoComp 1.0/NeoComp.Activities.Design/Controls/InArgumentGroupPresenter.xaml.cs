using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Activities.Presentation.Model;
using NeoComp.Activities.Design.ViewModel;
using System.Activities.Presentation;
using System.Collections.Specialized;
using System.Activities.Presentation.View;
using System.Activities.Presentation.Converters;
using NeoComp.WPF.Helpers;

namespace NeoComp.Activities.Design.Controls
{
    /// <summary>
    /// Interaction logic for PropertyGroupPresenter.xaml
    /// </summary>
    public partial class InArgumentGroupPresenter : UserControl
    {
        public InArgumentGroupPresenter()
        {
            InitializeComponent();
        }

        public bool IsSharedSize
        {
            get { return (bool)GetValue(IsSharedSizeProperty); }
            set { SetValue(IsSharedSizeProperty, value); }
        }

        public static readonly DependencyProperty IsSharedSizeProperty =
            DependencyProperty.Register("IsSharedSize", typeof(bool), typeof(InArgumentGroupPresenter), new UIPropertyMetadata(true));

        public string GroupName
        {
            get { return (string)GetValue(GroupNameProperty); }
            set { SetValue(GroupNameProperty, value); }
        }

        public static readonly DependencyProperty GroupNameProperty =
            DependencyProperty.Register("GroupName", typeof(string), typeof(InArgumentGroupPresenter),
                new UIPropertyMetadata(null, (obj, e) => ((InArgumentGroupPresenter)obj).OnGroupNamePropertyChanged(e)));

        public object ActivityDesigner
        {
            get { return (object)GetValue(ActivityDesignerProperty); }
            set { SetValue(ActivityDesignerProperty, value); }
        }

        public static readonly DependencyProperty ActivityDesignerProperty =
            DependencyProperty.Register("ActivityDesigner", typeof(object), typeof(InArgumentGroupPresenter),
                new UIPropertyMetadata(null, (obj, e) => ((InArgumentGroupPresenter)obj).OnActivityDesignerPropertyChanged(e)));

        private void OnGroupNamePropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            Changed();
        }

        private void OnActivityDesignerPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            Changed();
        }

        private void Changed()
        {
            var ad = ActivityDesigner as ActivityDesigner;
            var mi = ad != null ? ad.ModelItem : null;
            if (!string.IsNullOrEmpty(GroupName) && mi != null)
            {
                DataContext = new InArgumentExpressionGroup(mi, GroupName);
            }
            else
            {
                DataContext = null;
            }
        }

        private void ExpBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (!addBindingsScheduled)
            {
                addBindingsScheduled = true;
                Dispatcher.BeginInvoke(new Action(AddBindings));
            }
        }

        bool addBindingsScheduled;

        private void AddBindings()
        {
            var conv = new ArgumentToExpressionConverter();
            var g = DataContext as InArgumentExpressionGroup;
            if (g != null)
            {
                foreach (var item in g.Expressions)
                {
                    var cp = Items.ItemContainerGenerator.ContainerFromItem(item) as ContentPresenter;
                    var extb = WPFTree.FindVisualChild<ExpressionTextBox>(cp);
                    if (extb != null)
                    {
                        var binding = new Binding
                        {
                            Path = new PropertyPath("OwnerModelItem." + item.Name),
                            Mode = BindingMode.TwoWay,
                            Converter = conv,
                            ConverterParameter = "In"
                        };
                        extb.SetBinding(ExpressionTextBox.ExpressionProperty, binding);
                    }
                }
            }
            addBindingsScheduled = false;
        }
    }
}
