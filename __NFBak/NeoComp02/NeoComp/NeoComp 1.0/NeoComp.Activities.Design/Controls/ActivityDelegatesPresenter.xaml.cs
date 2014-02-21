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
using System.Activities.Presentation;
using NeoComp.Activities.Design.ViewModel;

namespace NeoComp.Activities.Design.Controls
{
    /// <summary>
    /// Interaction logic for ActivityDelegatesPresenter.xaml
    /// </summary>
    public partial class ActivityDelegatesPresenter : UserControl
    {
        public ActivityDelegatesPresenter()
        {
            InitializeComponent();
        }

        public object ActivityDesigner
        {
            get { return (object)GetValue(ActivityDesignerProperty); }
            set { SetValue(ActivityDesignerProperty, value); }
        }

        public static readonly DependencyProperty ActivityDesignerProperty =
            DependencyProperty.Register("ActivityDesigner", typeof(object), typeof(ActivityDelegatesPresenter),
                new UIPropertyMetadata(null, (obj, e) => ((ActivityDelegatesPresenter)obj).OnActivityDesignerPropertyChanged(e)));

        private void OnActivityDesignerPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            var ad = e.NewValue as ActivityDesigner;
            var mi = ad != null ? ad.ModelItem : null;
            if (mi == null)
            {
                DataContext = null;
            }
            else
            {
                DataContext = new ActivityDelegateItems(mi);
            }
        }
    }
}
